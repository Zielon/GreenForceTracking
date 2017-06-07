using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Server.API;
using Server.Common;
using Server.Frames;
using Server.Frames.Client;

namespace TestingClient
{
    class Client
    {
        private readonly TcpClient _client;

        private double count = 1.1;
        public IPAddress Ip;
        public bool isConnedted = false;
        public IProgress<string> Progress;
        private readonly ManualResetEvent waitForSocket = new ManualResetEvent(true);
        public MainWindow Window;

        public Client(Progress<string> progress, MainWindow window)
        {
            _client = new TcpClient();

            Window = window;
            Progress = progress;
        }

        public void Conntect()
        {
            if (_client.Connected) return;

            var check = Window.customIPBtn.IsChecked ?? false;
            if (check) { Ip = IPAddress.Parse(Window.Ip_Box.Text); }
            else
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry("kornik.ddns.net");
                for (int i = 0; i < ipHostInfo.AddressList.Length; ++i)
                    if (ipHostInfo.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        Ip = ipHostInfo.AddressList[i];
                        break;
                    }
            }

            _client.ConnectAsync(Ip, 52400);
        }

        public async void StartProcessing()
        {
            while (true)
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
                            using (var sw = new StringWriter())
                            {
                                using (var xw = new XmlTextWriter(sw))
                                {
                                    xw.Formatting = Formatting.Indented;
                                    xw.Indentation = 2;
                                    player.WriteTo(xw);
                                }

                                var xml = sw.ToString();
                                var f = FramesFactory.CreateObject<Server.Common.Client>(xml);
                                str +=
                                    $"User: {f.Login}\nAcc: {f.Accuracy}\nLat: {f.Lat}\nLng: {f.Lng}\nMsg: {f.Message}\n--------------------\n";
                            }

                        foreach (XmlNode player in GetNodeList(message, "RemoveUser"))
                            using (var sw = new StringWriter())
                            {
                                using (var xw = new XmlTextWriter(sw))
                                {
                                    xw.Formatting = Formatting.Indented;
                                    xw.Indentation = 2;
                                    player.WriteTo(xw);
                                }

                                var xml = sw.ToString();
                                var f = FramesFactory.CreateObject<RemoveUser>(xml);
                                str += $"User: {f.Login} was deleted\n--------------------\n";
                            }

                        foreach (XmlNode player in GetNodeList(message, "Marker"))
                            using (var sw = new StringWriter())
                            {
                                using (var xw = new XmlTextWriter(sw))
                                {
                                    xw.Formatting = Formatting.Indented;
                                    xw.Indentation = 2;
                                    player.WriteTo(xw);
                                }

                                var xml = sw.ToString();
                                var f = FramesFactory.CreateObject<Marker>(xml);
                                str += $"User: {f.Login} added new marker\n--------------------\n";
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

            var msg = new SystemUser { Login = user, Password = password };

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

                var user = new Server.Common.Client
                {
                    Posision = new Posision(54.408938381 + count, 19.566548634 - count),
                    Accuracy = 1.53,
                    Direction = 5.4,
                    Login = Window.NameBox.Text,
                    Message = data,
                    FrameType = Frames.Player
                };

                string msg = FramesFactory.CreateXmlMessage(user);

                await writer.WriteLineAsync(msg);
                count += 0.0000023;

                waitForSocket.Set();
            }
            catch (Exception ex) { Window.textBoxResponse.Text = ex.Message + "\n" + ex.StackTrace; }
        }

        public async Task SendMarker()
        {
            try
            {
                waitForSocket.Reset();

                NetworkStream networkStream = _client.GetStream();
                StreamWriter writer = new StreamWriter(networkStream);

                writer.AutoFlush = true;
                Window.textBoxResponse.Clear();

                var marker = new Marker
                {
                    Add = true,
                    Login = Window.NameBox.Text,
                    Points =
                        new List<Posision>
                        {
                            new Posision(1.1, 2.3),
                            new Posision(3.1, 2.3),
                            new Posision(4.1, 2.3),
                            new Posision(5.1, 2.3)
                        },
                    FrameType = Frames.Marker,
                    Text = "New marker added",
                    Color = -234,
                    Outside = false,
                    Type = "area",
                    Id = Tools.RandomString()
                };

                string msg = FramesFactory.CreateXmlMessage(marker);

                await writer.WriteLineAsync(msg);
                count += 11.6;

                waitForSocket.Set();
            }
            catch (Exception ex) { Window.textBoxResponse.Text = ex.Message + "\n" + ex.StackTrace; }
        }
    }
}
