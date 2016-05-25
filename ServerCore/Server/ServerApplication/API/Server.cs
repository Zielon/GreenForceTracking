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
                {
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
                                break;
                            case Frames.Frames.Login:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }

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

            container.RecivedMessages.Add(new Message()
            {
                Adress = client.IpAddress,
                Time = DateTime.Now,
                RecivedData = client.Message
            });
        }
    }
}
