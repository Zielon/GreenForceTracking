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
                listener = new TcpListener(IPAddress.Parse("192.168.0.2"), Consts.SendingPort);
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
                        XmlNodeList elements = doc.GetElementsByTagName("Player");
                        string str = string.Empty;

                        foreach (XmlNode player in elements)
                        {
                            var id = player["ID"].InnerText;
                            var lat = Double.Parse(player["Lat"].InnerText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture);
                            var lon = Double.Parse(player["Lon"].InnerText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture);
                            var user = player["User"].InnerText;

                            str = string.Format("User: {0}\nID: {1}\nLat: {2}\nLon: {3}",
                                                     user, id, lat, lon);
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

                var user = new ServerApplication.Client()
                {
                    ID = "1",
                    IpAddress = Ip,
                    Posision = new Tuple<double, double>(1.23 + count, 543.456 - count),
                    UserName = "Testing Client",
                    Message = data
                };

                await writer.WriteLineAsync(user.ToString());
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
