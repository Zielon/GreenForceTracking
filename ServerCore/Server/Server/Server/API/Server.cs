using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using Library.Frames.Server;
using Library.Common;
using Library.Frames.Factory;
using Library.Events;
using Library.Messages;
using Server.API;
using Library.Frames;
using Library.Frames.Client;
using Server.Mock;
using System.Linq;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Library.Server
{
    public class Server
    {
        public MessagesContainer Container { get; private set; }
        public EventHandler<MessageEventArgs> MessageEvent;
        public EventHandler<ContainerEventArgs> ContainerEvent;
        public EventHandler<WindowEventArgs> WindowEvent;
        public Room Room = new Room();

        private IPAddress ipAddress;
        private int port;
        private static EventWaitHandle waitHandle = new AutoResetEvent(false);
        private List<TcpClient> clientList = new List<TcpClient>();
        private MessagesHandler FramesHandler;

        public Server(string ipAddress, int port)
        {
            this.ipAddress = IPAddress.Parse(ipAddress);
            this.port = port;
            Container = new MessagesContainer();
            FramesHandler = new MessagesHandler(this);
        }

        public void ClientPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Client item = sender as Client;
            Marker marker = sender as Marker;
            SystemUser login = sender as SystemUser;

            if (item != null){
                StartSending(new RoomInfoServer(){ Client = item, Login = item.Login });
            }
            else if (marker != null){
                StartSending(new MarkerInfoServer() { Marker = marker, Login = marker.Login });
            }
            else if (login != null){
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
                {
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
                }

                notConnected.ForEach(e => {
                    Room.Players.Remove(e);
                    Room.Markers.Where(m => m.Login == e.Login).ToList().ForEach(m => Room.Markers.Remove(m));
                    StartSending(
                        new RemoveUser { Connection = e.Connection, FrameType = Frames.Frames.RemovingUser, Login = e.Login });
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
                        FramesHandler.ParseMessage(message, localClient);
                    }
                    else break;
                }

            }, TaskCreationOptions.LongRunning);
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