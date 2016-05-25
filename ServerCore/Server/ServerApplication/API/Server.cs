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
using System.Globalization;
using System.ComponentModel;
using ServerApplication.Frames.Server;
using ServerApplication.Common;
using ServerApplication.Frames.Factory;

namespace ServerApplication
{
    public class Server
    {
        private IPAddress ipAddress;
        private int port;
        private MainWindow window;
        private List<Room> Rooms = new List<Room>();
        private static EventWaitHandle waitHandle = new AutoResetEvent(false);

        public static bool isRunning = false;
        public MessagesContainer container = new MessagesContainer();

        public Server(string ipAddress, int port, MainWindow mainwindow)
        {
            this.ipAddress = IPAddress.Parse(ipAddress);
            this.port = port;
            this.window = mainwindow;
            window.dataGrid.DataContext = container.RecivedMessages;

            Console.WriteLine("Listening on port: " + port);

            //TODO temporaty solution
            var room = new Room("1");
            room.Players.CollectionChanged += UpdatePlayers;

            Rooms.Add(room);
        }

        public void UpdatePlayers(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                Console.WriteLine("Players in room: " + Rooms.First().Players.Count);

            }
        }

        void ClientPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Client item = sender as Client;
            if (item != null)
                 Task.Factory.StartNew(() => StartSending(item));
        }

        /// <summary>
        /// Running in separte thread.
        /// </summary>
        public void StartSending(Client player)
        {
            try
            {
                TcpClient client = new TcpClient();

                lock (Rooms)
                {
                    var selectedRoom = Rooms.Single(r => r.ID == player.RoomId);
                    selectedRoom.Players.ToList().ForEach(async p =>
                    {
                        await client.ConnectAsync(p.IpAddress, Consts.SendingPort);
                        NetworkStream networkStream = client.GetStream();
                        StreamWriter writer = new StreamWriter(networkStream);
                        writer.AutoFlush = true;

                        var msg = FramesFactory.CreateXmlMessage(
                            new RoomInfoServer() { Players = selectedRoom.Players.ToList() });

                        await writer.WriteLineAsync(msg);
                        client.Close();
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
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
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
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
                    // Read one single line
                    string message = await reader.ReadLineAsync();

                    if (message != null)
                    {
                        string clientEndPoint = tcpClient.Client.RemoteEndPoint
                            .ToString().Split(':').First();

                        var recivedData = ParseMessage(message, clientEndPoint);

                        if (recivedData != null)
                            //Display message
                            container.RecivedMessages.Add(new Message()
                            {
                                Adress = IPAddress.Parse(clientEndPoint),
                                Time = DateTime.Now,
                                RecivedData = recivedData.Message
                            });
                    }
                    else break; // Closed connection
                }


                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
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
                    var lat = Double.Parse(player["Lat"].InnerText.Replace(',', '.'),
                        NumberStyles.Any, CultureInfo.InvariantCulture);
                    var lon = Double.Parse(player["Lon"].InnerText.Replace(',', '.'),
                        NumberStyles.Any, CultureInfo.InvariantCulture);
                    var message = player["Message"].InnerText;
                    var user = player["User"].InnerText;

                    client = new Client()
                    {
                        ID = id,
                        Posision = new Posision(lat, lon),
                        Message = message,
                        UserName = user,
                        IpAddress = ipAddress,
                        RoomId = "1"
                    };

                    client.PropertyChanged += ClientPropertyChanged;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }

            if (client == null) return null;

            //TODO Fix update rooms
            if (!Rooms.First().Players.Contains(client))
            {
                ClientPropertyChanged(client, new PropertyChangedEventArgs("Posision"));
                Rooms.First().Players.Add(client);
            }
            else
            {
                var player = Rooms.First().Players.First();
                // Send new possision by INotifyPropertyChanged mechanism

                // Check if posision was changed, if no dont update
                player.Posision = new Posision(client.Lat, client.Lon);
                player.Message = client.Message;
                client = player;
            }


            return client;
        }
    }
}
