using System;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml;
using System.Collections.Specialized;
using System.Threading;
using System.Text.RegularExpressions;

namespace ServerApplication
{
    public class Server
    {
        private IPAddress ipAddress;
        private int port;
        private MainWindow window;
        public static bool isRunning = false;

        private List<Room> Rooms = new List<Room>();

        public MessagesContainer container = new MessagesContainer();

        public Server(string ipAddress, int port, MainWindow mainwindow)
        {
            this.ipAddress = IPAddress.Parse(ipAddress);
            this.port = port;
            this.window = mainwindow;
            window.dataGrid.DataContext = container.RecivedMessages;

            //TODO temporaty solution
            var room = new Room();
            room.Players.CollectionChanged += UpdatePlayers;

            Rooms.Add(room);
        }

        public void UpdatePlayers(object sender, NotifyCollectionChangedEventArgs e)
        {
            // To be checked
            new Thread(() => StartSending()).Start();
        }

        // TODO start sending data to clients
        public void StartSending()
        {
            try
            {
                TcpClient client = new TcpClient();

                Rooms.First().Players.ToList().ForEach(async p =>
                {

                    await client.ConnectAsync(p.IpAddress, 52300);

                    NetworkStream networkStream = client.GetStream();
                    StreamWriter writer = new StreamWriter(networkStream);
                    writer.AutoFlush = true;

                    await writer.WriteLineAsync(p.ToString());
                    client.Close();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async void StartListening()
        {
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(this.ipAddress, this.port);
                listener.Start();
                isRunning = true;
                window.ServerStatus.Content = "Server is running...";
                window.ServerStatus.Foreground = new SolidColorBrush(Colors.Green);

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

        private async Task Process(TcpClient tcpClient)
        {
            try
            {
                NetworkStream networkStream = tcpClient.GetStream();
                StreamReader reader = new StreamReader(networkStream, true);

                while (true)
                {
                    string message = await reader.ReadLineAsync();

                    if (message != null)
                    {
                        string clientEndPoint = tcpClient.Client.RemoteEndPoint.ToString().Split(':').First();
                        var recivedData = ParseMessage(message.ToString(), clientEndPoint);

                        if (recivedData != null)
                            container.RecivedMessages.Add(new Message()
                            {
                                Adress = IPAddress.Parse(clientEndPoint),
                                Time = DateTime.Now,
                                RecivedData = recivedData.Message
                            });
                    }
                    else
                        break; // Closed connection
                }

                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (tcpClient.Connected) tcpClient.Close();
            }
        }

        private Client ParseMessage(string msg, string ip)
        {
            XmlDocument doc = new XmlDocument();
            Client client = null;

            try
            {
                doc.LoadXml(msg);
                XmlNodeList elements = doc.GetElementsByTagName("Player");
                var ipAddress = IPAddress.Parse(ip);

                foreach (XmlNode player in elements)
                {
                    var id = player["ID"].InnerText;
                    var lat = Double.Parse(player["Lat"].InnerText);
                    var lon = Double.Parse(player["Lon"].InnerText);
                    var message = player["Message"].InnerText;
                    var user = player["User"].InnerText;

                    client = new Client()
                    {
                        ID = id,
                        Lat = lat,
                        Lon = lon,
                        Message = message,
                        UserName = user,
                        IpAddress = ipAddress
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (client == null) return null;

            if (!Rooms.First().Players.Contains(client))
                Rooms.First().Players.Add(client);
            else
            {
                var player = Rooms.First().Players.First();
                player.Lat = client.Lat;
                player.Lon = client.Lon;
                client = player;
            }

            return client;
        }
    }
}
