using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Xml;
using System.Collections.Specialized;
using System.Threading;
using System.ComponentModel;
using Library.Frames.Server;
using Library.Common;
using Library.Frames.Factory;
using Library.API;
using Library.Events;
using Library.Messages;

namespace Library.Server
{
    public class Server
    {
        public MessagesContainer Container { get; private set; }
        public EventHandler<MessageEventArgs> MessageEvent;
        public EventHandler<ContainerEventArgs> ContainerEvent;
        public EventHandler<WindowEventArgs> WindowEvent;

        private IPAddress ipAddress;
        private int port;
        private static EventWaitHandle waitHandle = new AutoResetEvent(false);
        private List<TcpClient> clientList = new List<TcpClient>();
        private Room Room = new Room();

        public Server(string ipAddress, int port)
        {
            this.ipAddress = IPAddress.Parse(ipAddress);
            this.port = port;
            Container = new MessagesContainer();
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
                        OnMessageChange(new MessageEventArgs { Message = "Method: StartSending()\n" });
                        OnMessageChange(new MessageEventArgs { Message = $"User: {p.UserName}\n" });
                        OnMessageChange(new MessageEventArgs { Message = ex.Message + "\n" + ex.StackTrace });
                    }
                }

                notConnected.ForEach(e =>
                {
                    Room.Players.Remove(e);
                    OnMessageChange(new MessageEventArgs { Message = $"{e.UserName} has been deleted\n" });
                });
            }
        }

        public async void StartListening()
        {
            TcpListener listener = null;

            try
            {
                listener = new TcpListener(this.ipAddress, this.port);
                listener.Start();
                OnMessageChange(new MessageEventArgs { Message = $"Listening on port: {port}\n" });
                OnWindowChange(new WindowEventArgs { Running = "Server is running...", ChangeBrush = true });

                while (true)
                {
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                    Processing(tcpClient);
                }
            }
            catch (Exception ex)
            {
                OnMessageChange(new MessageEventArgs { Message = ex.Message + "\n" + ex.StackTrace });
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
                                break;
                            default:
                                throw new InvalidEnumArgumentException();
                        }
                    }
            }
            catch (Exception ex)
            {
                OnMessageChange(new MessageEventArgs { Message = ex.Message + "\n" + ex.StackTrace });
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

                    OnMessageChange(new MessageEventArgs { Message = $"New player {client.UserName}\n" });
                }
                else
                {
                    playerInTheRoom = Room.Players.Single(p => p.UserName.Equals(client.UserName));
                    if (!client.Posision.Equals(playerInTheRoom.Posision))
                        playerInTheRoom.Posision = client.Posision;
                    playerInTheRoom.Message = client.Message;
                }
            }

            OnContainerChange(new ContainerEventArgs
            {
                Message = new Message()
                {
                    User = playerInTheRoom.UserName,
                    Adress = tcpClient.Client.RemoteEndPoint.ToString(),
                    Time = DateTime.Now,
                    RecivedData = client.Message
                }
            });

            lock (Container.RecivedMessages)
                 if (Container.RecivedMessages.Count > 17)
                    OnContainerChange(new ContainerEventArgs { Clean = true });
        }

        private void OnMessageChange(MessageEventArgs args)
        {
            MessageEvent?.Invoke(this, args);
        }

        private void OnContainerChange(ContainerEventArgs args)
        {
            ContainerEvent?.Invoke(this, args);
        }

        private void OnWindowChange(WindowEventArgs args)
        {
            WindowEvent?.Invoke(this, args);
        }
    }
}