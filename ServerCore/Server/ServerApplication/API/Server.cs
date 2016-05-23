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

        private static EventWaitHandle waitHandle = new AutoResetEvent(false);

        public Server(string ipAddress, int port, MainWindow mainwindow)
        {
            this.ipAddress = IPAddress.Parse(ipAddress);
            this.port = port;
            this.window = mainwindow;
            window.dataGrid.DataContext = container.RecivedMessages;

            Console.WriteLine("Listening on port: " + port);

            //TODO temporaty solution
            var room = new Room();
            room.Players.CollectionChanged += UpdatePlayers;

            Rooms.Add(room);
        }

        public void UpdatePlayers(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                Console.WriteLine("Players in room: " + Rooms.First().Players.Count);
                waitHandle.Set(); //Posision to update
            }
        }

        void ClientPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Client item = sender as Client;
            if (item != null)
                waitHandle.Set(); //Posision to update
        }

        public void StartSending()
        {
            try
            {
                while (true)
                {
                    waitHandle.WaitOne();  //Wait for a signal
                    TcpClient client = new TcpClient();

                    lock (Rooms)
                    {
                        Rooms.First().Players.ToList().ForEach(async p =>
                        {
                            await client.ConnectAsync(p.IpAddress, Consts.SendingPort);
                            NetworkStream networkStream = client.GetStream();
                            StreamWriter writer = new StreamWriter(networkStream);
                            writer.AutoFlush = true;

                            p.UserName = "Server";

                            await writer.WriteLineAsync(p.ToString());
                            client.Close();
                        });
                    }
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
                        SessionID = id,
                        Posision = new Posision(lat, lon),
                        Message = message,
                        UserName = user,
                        IpAddress = ipAddress
                    };

                    client.PropertyChanged += ClientPropertyChanged;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }

            if (client == null) return null;

            lock (Rooms)
            {
                //TODO Fix update rooms
                if (!Rooms.First().Players.Contains(client))
                    Rooms.First().Players.Add(client);
                else
                {
                    var player = Rooms.First().Players.First();
                    // Send new possision by INotifyPropertyChanged mechanism
                    player.Posision = new Posision(client.Lat, client.Lon);
                    client = player;
                }
            }

            return client;
        }
    }
}
