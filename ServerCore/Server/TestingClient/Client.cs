using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Xml;
using Library.Common;
using Library.Frames;
using Library.Frames.Factory;
using System.Threading;

namespace TestingClient
{
    class Client
    {
        public MainWindow Window;
        public IPAddress Ip;
        public bool isConnedted = false;
        public IProgress<string> Progress;

        private double count = 1.1;
        private ManualResetEvent waitForSocket = new ManualResetEvent(true);
        private TcpClient _client;

        public Client(Progress<string> progress, MainWindow window)
        {
            _client = new TcpClient();

            this.Window = window;
            this.Progress = progress;

            IPHostEntry ipHostInfo = Dns.GetHostEntry("kornik.ddns.net");

            for (int i = 0; i < ipHostInfo.AddressList.Length; ++i)
            {
                if (ipHostInfo.AddressList[i].AddressFamily ==
                  AddressFamily.InterNetwork)
                {
                    Ip = ipHostInfo.AddressList[i];
                    break;
                }
            }

            //Ip = IPAddress.Parse("192.168.0.2");
            _client.ConnectAsync(Ip, 52400);
        }

        public async void StartProcessing()
        {
            while (true)
            {
                try
                {
                    if (!_client.Connected) _client.Connect(Ip, 52400);

                    NetworkStream networkStream = _client.GetStream();
                    StreamReader reader = new StreamReader(networkStream);

                    string message = await reader.ReadLineAsync();

                    if (message != null)
                    {
                        string str = string.Empty;

                        foreach (XmlNode players in GetNodeList(message, "Players"))
                        {
                            foreach (XmlNode player in players)
                            {
                                using (var sw = new StringWriter())
                                {
                                    using (var xw = new XmlTextWriter(sw))
                                    {
                                        xw.Formatting = System.Xml.Formatting.Indented;
                                        xw.Indentation = 2;
                                        player.WriteTo(xw);
                                    }

                                    var xml = sw.ToString();
                                    var f = FramesFactory.CreateObject<Library.Common.Client>(xml);
                                    str += string.Format(
                                        "User: {0}\nID: {1}\nLat: {2}\nLon: {3}\nMsg: {4}\n--------------------\n",
                                        f.Login, f.ID, f.Lat, f.Lon, f.Message);
                                }
                            }
                        }

                        Progress.Report(str);
                    }
                }
                catch (Exception ex)
                {
                    Progress.Report(ex.Message + "\n" + ex.StackTrace);
                }
            }
        }

        private XmlNodeList GetNodeList(string xml, string tagName)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc.GetElementsByTagName(tagName);
        }

        public async Task<bool> Login(string user, string password)
        {
            NetworkStream networkStream = _client.GetStream();
            StreamWriter writer = new StreamWriter(networkStream);

            writer.AutoFlush = true;

            var msg = new Library.Frames.Client.SystemUser
            {
                Login = user,
                Password = password
            };

            await writer.WriteLineAsync(FramesFactory.CreateXmlMessage(msg));

            StreamReader reader = new StreamReader(networkStream);

            string logger = await reader.ReadLineAsync();

            XmlNodeList elements = GetNodeList(logger, "LoggedIn");

            return bool.Parse(elements[0].InnerText);
        }

        public async Task Send(string data)
        {
            try
            {
                waitForSocket.Reset();

                NetworkStream networkStream = _client.GetStream();
                StreamWriter writer = new StreamWriter(networkStream);

                writer.AutoFlush = true;
                Window.textBoxResponse.Clear();

                var user = new Library.Common.Client()
                {
                    Posision = new Posision(1.23 + count, 543.456 - count),
                    Login = Window.NameBox.Text,
                    Message = data,
                    FrameType = Frames.Player
                };

                string msg = FramesFactory.CreateXmlMessage(user);

                await writer.WriteLineAsync(msg);
                count += 11.6;

                waitForSocket.Set();
            }
            catch (Exception ex)
            {
                Window.textBoxResponse.Text = ex.Message + "\n" + ex.StackTrace;
            }
        }
    }
}
