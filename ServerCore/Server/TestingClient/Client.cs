using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ServerApplication;
using ServerApplication.Frames;
using ServerApplication.Frames.Factory;
using ServerApplication.Common;

namespace TestingClient
{
    class Client
    {
        public static MainWindow window;
        public static IPAddress Ip;
        public static int Port;
        public static bool isConnedted = false;
        private static double count = 1.1;


        public static async void StartListening()
        {
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(IPAddress.Parse(window.textBoxIP_Copy.Text), Consts.SendingPort);
                listener.Start();

                while (true)
                {
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                    Task t = Process(tcpClient);
                    await t;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static async Task Process(TcpClient tcpClient)
        {
            try
            {
                NetworkStream networkStream = tcpClient.GetStream();
                StreamReader reader = new StreamReader(networkStream);

                while (true)
                {
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

                        window.textBoxResponse.Text = str;
                    }
                    else break; // Closed connection
                }

                tcpClient.Close();
            }
            catch (Exception ex)
            {
                if (tcpClient.Connected) tcpClient.Close();
            }
        }

        public static async Task Send(string data)
        {

            if (!isConnedted) return;

            try
            {
                TcpClient client = new TcpClient();

                await client.ConnectAsync(Ip, Port);

                NetworkStream networkStream = client.GetStream();
                StreamWriter writer = new StreamWriter(networkStream);

                writer.AutoFlush = true;

                var user = new ServerApplication.Common.Client()
                {
                    RoomId = window.RoomBox.Text,
                    IpAddress = Ip,
                    Posision = new Posision(1.23 + count, 543.456 - count),
                    UserName = window.NameBox.Text,
                    Message = data,
                    FrameType = Frames.Player
                };

                string msg = FramesFactory.CreateXmlMessage(user);

                await writer.WriteLineAsync(msg);
                count += 11.6;
                client.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
