using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Xml;
using ServerApplication.Frames;
using ServerApplication.Frames.Factory;
using ServerApplication.Common;

namespace TestingClient
{
    class Client
    {
        public static MainWindow window;
        public static IPAddress Ip;
        public static bool isConnedted = false;
        private static double count = 1.1;

        static TcpClient _client;

        static Client()
        {
            _client = new TcpClient();

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

            Ip = IPAddress.Parse("192.168.0.3");
            _client.ConnectAsync(Ip, 52400);
        }

        public static async void Process(IProgress<string> update)
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
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(message);
                        XmlNodeList elements = doc.GetElementsByTagName("Players");
                        string str = string.Empty;

                        foreach (XmlNode players in elements)
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
                                    var f = FramesFactory.CreateObject<ServerApplication.Common.Client>(xml);
                                    str += string.Format(
                                        "User: {0}\nID: {1}\nLat: {2}\nLon: {3}\nMsg: {4}\n--------------------\n",
                                        f.UserName, f.ID, f.Lat, f.Lon, f.Message);
                                }
                            }
                        }

                        update.Report(str);
                    }
                }
                catch (Exception ex)
                {
                    update.Report(ex.Message);

                    if (_client.Connected) _client.Close();
                }
            }
        }

        public static async Task Send(string data)
        {
            try
            {
                NetworkStream networkStream = _client.GetStream();
                StreamWriter writer = new StreamWriter(networkStream);

                writer.AutoFlush = true;

                var user = new ServerApplication.Common.Client()
                {
                    Posision = new Posision(1.23 + count, 543.456 - count),
                    UserName = window.NameBox.Text,
                    Message = data,
                    FrameType = Frames.Player
                };

                string msg = FramesFactory.CreateXmlMessage(user);

                await writer.WriteLineAsync(msg);
                count += 11.6;
            }
            catch (Exception ex)
            {
                window.textBoxResponse.Text = ex.Message;
            }
        }
    }
}
