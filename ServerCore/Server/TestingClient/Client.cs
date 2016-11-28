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
        }

        public void Conntect() {

            if (_client.Connected) return;

            var check = Window.customIPBtn.IsChecked ?? false;
            if (check)
            {
                Ip = IPAddress.Parse(Window.Ip_Box.Text);
            }
            else
            {
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
            }

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

                        foreach (XmlNode player in GetNodeList(message, "Client"))
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
                                    "User: {0}\nAcc: {1}\nLat: {2}\nLng: {3}\nMsg: {4}\n--------------------\n",
                                    f.Login, f.Accuracy, f.Lat, f.Lng, f.Message);
                            }
                        }

                        Progress.Report(str);
                    }
                }
                catch (Exception ex)
                {
                    Progress.Report(ex.Message + "\n");
                    break;
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

            var xmlMsg = FramesFactory.CreateXmlMessage(msg);

            await writer.WriteLineAsync(xmlMsg);

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
                    Accuracy = 1.53 + count,
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
