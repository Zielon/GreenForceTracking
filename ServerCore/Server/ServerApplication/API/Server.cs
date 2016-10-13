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
        private static EventWaitHandle waitHandle = new AutoResetEvent(false);
        private List<TcpClient> clientList = new List<TcpClient>();
        private Room Room = new Room();

        private IProgress<Message> progress;
        private IProgress<string> messages;

        public Server(string ipAddress, int port, MainWindow mainwindow)
        {
            this.ipAddress = IPAddress.Parse(ipAddress);
            this.port = port;
            this.window = mainwindow;
            window.dataGrid.DataContext = container.RecivedMessages;

            progress = new Progress<Message>(s => container.RecivedMessages.Add(s));
            messages = new Progress<string>(s => this.window.textBox.AppendText(s + "\n"));

            this.window.textBox.Text += $"Listening on port: {port}\n";
        }

        public void UpdatePlayers(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                messages.Report("New player was added");
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
            lock (Room.Players)
            {
                var notConnected = new List<Client>();

                foreach (var p in Room.Players)
                {
                    try
                    {

                        if (p.Connection == null) continue;

                        TcpClient client = p.Connection;

                        if (!client.Connected) { notConnected.Add(p); continue; }

                        NetworkStream networkStream = client.GetStream();
                        StreamWriter writer = new StreamWriter(networkStream);

                        writer.AutoFlush = true;

                        var msg = FramesFactory.CreateXmlMessage(
                            new RoomInfoServer() { Players = Room.Players.ToList() });

                        writer.WriteLine(msg);
                    }
                    catch (Exception ex)
                    {
                        notConnected.Add(p);
                        messages.Report("Method: StartSending()\n");
                        messages.Report($"User: {p.UserName}\n");
                        messages.Report(ex.Message + "\n" + ex.StackTrace);
                    }
                }

                notConnected.ForEach(e => { Room.Players.Remove(e); messages.Report($"{e.UserName} has been deleted\n"); });
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
                    Processing(tcpClient);
                }
            }
            catch (Exception ex)
            {
                messages.Report(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void Processing(TcpClient client)
        {
            var localClient = client;

            Task.Factory.StartNew(async () =>
            {
                NetworkStream networkStream = localClient.GetStream();
                StreamReader reader = new StreamReader(networkStream, true);

                while (true)
                {
                    string message = await reader.ReadLineAsync();

                    if (message != null)
                    {
                        ParseMessage(message, localClient);
                    }
                    else break;
                }

            }, TaskCreationOptions.LongRunning);
        }

        private void ParseMessage(string msg, TcpClient tcpClient)
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
                            xw.Formatting = Formatting.Indented;
                            xw.Indentation = 2;
                            player.WriteContentTo(xw);
                            xml = sw.ToString();
                        }

                        switch (type)
                        {
                            case Frames.Frames.Player:
                                client = FramesFactory.CreateObject<Client>(xml);
                                client.PropertyChanged += ClientPropertyChanged;
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
                messages.Report(ex.Message + "\n" + ex.StackTrace);
            }

            Client playerInTheRoom = null;

            lock (Room.Players)
            {
                if (!Room.Players.Contains(client))
                {
                    Room.Players.Add(client);
                    client.Connection = tcpClient;
                    client.Posision = new Posision(client.Lat, client.Lon);
                    client.ID = Tools.RandomString();
                    playerInTheRoom = client;

                    messages.Report($"New player {client.UserName}");
                }
                else
                {
                    playerInTheRoom = Room.Players.Single(p => p.UserName.Equals(client.UserName));
                    if (!client.Posision.Equals(playerInTheRoom.Posision))
                        playerInTheRoom.Posision = client.Posision;
                    playerInTheRoom.Message = client.Message;
                }
            }

            progress.Report(new Message()
            {
                User = playerInTheRoom.UserName,
                Adress = tcpClient.Client.RemoteEndPoint.ToString(),
                Time = DateTime.Now,
                RecivedData = client.Message
            });

            lock (container.RecivedMessages)
                 if (container.RecivedMessages.Count > 17)
                    this.window.Dispatcher.Invoke(() => container.RecivedMessages.Remove(m => true));
        }
    }
}