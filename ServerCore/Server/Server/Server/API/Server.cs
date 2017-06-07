using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Library.Common;
using Library.Events;
using Library.Frames;
using Library.Frames.Client;
using Library.Frames.Factory;
using Library.Frames.Server;
using Library.Messages;
using Server.API;
using Server.Mock;

namespace Library.Server
{
    public class Server
    {
        private static EventWaitHandle waitHandle = new AutoResetEvent(false);
        private List<TcpClient> clientList = new List<TcpClient>();
        public EventHandler<ContainerEventArgs> ContainerEvent;
        private readonly MessagesHandler FramesHandler;

        private readonly IPAddress ipAddress;
        public EventHandler<MessageEventArgs> MessageEvent;
        private readonly int port;
        public Room Room = new Room();
        public EventHandler<WindowEventArgs> WindowEvent;

        public Server(string ipAddress, int port)
        {
            this.ipAddress = IPAddress.Parse(ipAddress);
            this.port = port;
            Container = new MessagesContainer();
            FramesHandler = new MessagesHandler(this);
        }

        public MessagesContainer Container { get; }

        public void ClientPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Client item = sender as Client;
            Marker marker = sender as Marker;
            SystemUser login = sender as SystemUser;

            if (item != null) { StartSending(new RoomInfoServer { Client = item, Login = item.Login }); }
            else if (marker != null) { StartSending(new MarkerInfoServer { Marker = marker, Login = marker.Login }); }
            else if (login != null)
            {
                if (!login.Connection.Connected) return;

                NetworkStream networkStream = login.Connection.GetStream();
                StreamWriter writer = new StreamWriter(networkStream);
                writer.AutoFlush = true;
                writer.WriteLine(FramesFactory.CreateXmlMessage(login));
            }
        }

        public void StartSending(IFrame frame)
        {
            ObservableCollection<IFrame> collection = Room.Players;

            lock (collection)
            {
                var notConnected = new List<IFrame>();

                foreach (var p in collection)
                    try
                    {
                        if (p.Connection == null || p.Login.Equals(frame.Login)) continue;

                        TcpClient client = p.Connection;

                        if (!client.Connected)
                        {
                            OnMessageChange(new MessageEventArgs { Message = $"User: {p.Login} is not connected !\n" });
                            notConnected.Add(p);
                            continue;
                        }

                        NetworkStream networkStream = client.GetStream();
                        StreamWriter writer = new StreamWriter(networkStream);

                        writer.AutoFlush = true;
                        string msg = string.Empty;

                        msg = FramesFactory.CreateXmlMessage(frame);
                        writer.WriteLine(msg);
                    }
                    catch (SocketException)
                    {
                        notConnected.Add(p);
                        OnMessageChange(new MessageEventArgs { Message = $"User: {p.Login} lost connection !\n" });
                    }
                    catch (Exception ex)
                    {
                        notConnected.Add(p);
                        OnMessageChange(new MessageEventArgs { Message = ex.Message + "\n" });
                    }

                notConnected.ForEach(
                    e => {
                        Room.Players.Remove(e);
                        Room.Markers.Where(m => m.Login == e.Login).ToList().ForEach(m => Room.Markers.Remove(m));
                        StartSending(new RemoveUser { Connection = e.Connection, FrameType = Frames.Frames.RemovingUser, Login = e.Login });
                        DataBaseMock.Users.Single(s => s.Login.Equals(e.Login)).LoggedIn = false;
                        OnMessageChange(new MessageEventArgs { Message = $"{e.Login} has been deleted\n" });
                    });
            }
        }

        public async void StartListening()
        {
            TcpListener listener = null;

            try
            {
                listener = new TcpListener(ipAddress, port);
                listener.Start();
                OnMessageChange(new MessageEventArgs { Message = $"Listening on port: {port}\n" });
                OnWindowChange(new WindowEventArgs { Running = "Server is running...", ChangeBrush = true });

                while (true)
                {
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                    Processing(tcpClient);
                }
            }
            catch (Exception ex) { OnMessageChange(new MessageEventArgs { Message = ex.Message + "\n" + ex.StackTrace }); }
        }

        private void Processing(TcpClient client)
        {
            var localClient = client;

            Task.Factory.StartNew(
                async () => {
                    NetworkStream networkStream = localClient.GetStream();
                    StreamReader reader = new StreamReader(networkStream, true);

                    while (true)
                    {
                        string message = await reader.ReadLineAsync();

                        if (message != null) FramesHandler.ParseMessage(message, localClient);
                        else break;
                    }
                },
                TaskCreationOptions.LongRunning);
        }

        public void OnMessageChange(MessageEventArgs args)
        {
            MessageEvent?.Invoke(this, args);
        }

        public void OnContainerChange(ContainerEventArgs args)
        {
            ContainerEvent?.Invoke(this, args);
        }

        public void OnWindowChange(WindowEventArgs args)
        {
            WindowEvent?.Invoke(this, args);
        }
    }
}
