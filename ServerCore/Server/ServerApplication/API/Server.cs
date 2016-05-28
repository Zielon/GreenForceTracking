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
using ServerApplication.Frames;
using ServerApplication.Frames.Server;
using ServerApplication.Common;
using ServerApplication.Frames.Factory;
using ServerApplication.API;
using System.Windows;
using ServerApplication.Frames.Client;

namespace ServerApplication
{
    public class Server
    {
        public static bool isRunning = false;
        public MessagesContainer container = new MessagesContainer();

        private IPAddress ipAddress;
        private int port;
        private MainWindow window;
        private Dictionary<string, Room> Rooms = new Dictionary<string, Room>();
        private static EventWaitHandle waitHandle = new AutoResetEvent(false);

        public Server(string ipAddress, int port, MainWindow mainwindow)
        {
            this.ipAddress = IPAddress.Parse(ipAddress);
            this.port = port;
            this.window = mainwindow;
            window.dataGrid.DataContext = container.RecivedMessages;

            Console.WriteLine("Listening on port: " + port);

            // TODO temporaty solution
            var room = new Room("1");
            room.Players.CollectionChanged += UpdatePlayers;

            Rooms.Add("1", room);
        }

        public void UpdatePlayers(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                Console.WriteLine("New player was added");
            }
        }

        void ClientPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Client item = sender as Client;
            if (item != null)
            {
                Task.Factory.StartNew(() => StartSending(item));
            }
        }

        public void StartSending(Client player)
        {
            try
            {
                lock (Rooms)
                {
                    var selectedRoom = Rooms[player.RoomId];

                    foreach (var p in selectedRoom.Players)
                    {
                        using (TcpClient client = new TcpClient())
                        {
                            var result = client.BeginConnect(p.IpAddress.ToString(), Consts.SendingPort, null, null);

                            if (!result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2)))
                                throw new TimeoutException();

                            NetworkStream networkStream = client.GetStream();
                            StreamWriter writer = new StreamWriter(networkStream);

                            writer.AutoFlush = true;

                            var msg = FramesFactory.CreateXmlMessage(
                                new RoomInfoServer() { Players = selectedRoom.Players.ToList() });

                            writer.WriteLine(msg);
                            client.EndConnect(result);
                        }
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
                        string clientEndPoint = tcpClient.Client.
                            RemoteEndPoint.ToString().Split(':').First();

                        ParseMessage(message, clientEndPoint);
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

        private void ParseMessage(string msg, string ip)
        {
            XmlDocument doc = new XmlDocument();
            Client client = null;

            try
            {
                doc.LoadXml(msg);

                XmlNodeList frame = doc.GetElementsByTagName("FrameType");
                var type = Tools.ParseEnum<Frames.Frames>(frame.Item(0).InnerText);

                string elements = string.Empty;

                foreach (XmlNode player in doc.GetElementsByTagName("Frame"))
                    using (var sw = new StringWriter())
                    {
                        string xml = string.Empty;

                        using (var xw = new XmlTextWriter(sw))
                        {
                            xw.Formatting = System.Xml.Formatting.Indented;
                            xw.Indentation = 2;
                            player.WriteContentTo(xw);
                            xml = sw.ToString();
                        }

                        switch (type)
                        {
                            case Frames.Frames.Player:
                                client = FramesFactory.CreateObject<Client>(xml);
                                client.PropertyChanged += ClientPropertyChanged;
                                client.IpAddress = IPAddress.Parse(ip);
                                client.Posision = new Posision(client.Lat, client.Lon);
                                break;
                            case Frames.Frames.Login:
                                SessionManager.Logger(FramesFactory.CreateObject<LoginClient>(xml));
                                break;
                            default:
                                throw new InvalidEnumArgumentException();
                        }
                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }

            lock (Rooms)
            {
                if (!Rooms[client.RoomId].Players.Contains(client))
                {
                    Rooms[client.RoomId].Players.Add(client);
                    client.Posision = new Posision(client.Lat, client.Lon);
                    client.ID = Tools.RandomString();
                }
                else
                {
                    // Send new possision by INotifyPropertyChanged mechanism
                    // Check if posision was changed. If not dont update
                    var player = Rooms[client.RoomId].Players.Single(p => p.UserName.Equals(client.UserName));
                    if (!client.Posision.Equals(player.Posision))
                        player.Posision = client.Posision;
                    player.Message = client.Message;
                }
            }

            // Add message to container
            container.RecivedMessages.Add(new Message()
            {
                Adress = client.IpAddress,
                Time = DateTime.Now,
                RecivedData = client.Message
            });

            // Clean messages container after 14 frames
            if (container.RecivedMessages.Count > 14)
                container.RecivedMessages.Remove(m => true);
        }
    }
}