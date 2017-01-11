using Library.API;
using Library.Common;
using Library.Events;
using Library.Frames;
using Library.Frames.Client;
using Library.Frames.Factory;
using Library.Messages;
using Server.Mock;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Xml;


namespace Server.API
{
    public class MessagesHandler
    {
        private Library.Server.Server Server;

        public MessagesHandler(Library.Server.Server server)
        {
            this.Server = server;
        }

        public void ParseMessage(string msg, TcpClient tcpClient)
        {
            XmlDocument doc = new XmlDocument();

            try
            {
                doc.LoadXml(msg);

                XmlNodeList frame = doc.GetElementsByTagName("FrameType");
                var type = Tools.ParseEnum<Frames>(frame.Item(0).InnerText);

                string elements = string.Empty;

                foreach (XmlNode player in doc.GetElementsByTagName("Frame"))
                {
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
                            case Frames.Player:
                                AddPlayer(xml, tcpClient);
                                break;
                            case Frames.Login:
                                CheckLogin(xml, tcpClient);
                                break;
                            case Frames.Marker:
                                AddMarker(xml, tcpClient);
                                break;
                            default:
                                throw new InvalidEnumArgumentException();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Server.OnMessageChange(new MessageEventArgs { Message = ex.Message + "\n" + ex.StackTrace });
            }
        }

        private void AddMarker(string xml, TcpClient tcpClient)
        {
            Marker marker = FramesFactory.CreateObject<Marker>(xml);

            if (!IsLogged(marker.Login)) return;

            Stats.StatsHandler.AddMarker(marker.Login);

            lock (Server.Room.Markers)
            {
                if (!Server.Room.Markers.Contains(marker) && marker.Add)
                {
                    Server.Room.Markers.Add(marker);
                    marker.PropertyChanged += Server.ClientPropertyChanged;
                    marker.Connection = tcpClient;
                    marker.NotifyPropertyChanged();

                    lock (Server.Container.RecivedMessages)
                         if (Server.Container.RecivedMessages.Count > 17)
                            Server.OnContainerChange(new ContainerEventArgs { Clean = true });

                }
                else if (Server.Room.Markers.Contains(marker) && !marker.Add)
                {
                    Marker m = Server.Room.Markers.Cast<Marker>().Single(e => e.Id == marker.Id);
                    m.Add = false;
                    m.NotifyPropertyChanged();
                    Server.Room.Markers.Remove(m);
                    Server.OnMessageChange(new MessageEventArgs { Message = $"Marker {m.Id} has been deleted !\n" });
                }
            }
        }

        private void CheckLogin(string xml, TcpClient tcpClient)
        {
            var client = FramesFactory.CreateObject<SystemUser>(xml);

            lock (DataBaseMock.Users)
            {
                var user = DataBaseMock.Users.SingleOrDefault(u => u.Password.Equals(client.Password) && u.Login.Equals(client.Login));
                if (user == null)
                {
                    client.PropertyChanged += Server.ClientPropertyChanged;
                    client.Connection = tcpClient;
                    client.LoggedIn = false;
                    return;
                }

                if (user != null)
                {
                    user.PropertyChanged += Server.ClientPropertyChanged;
                    user.Connection = tcpClient;
                    user.LoggedIn = true;
                }
            }
        }

        private bool IsLogged(string player)
        {
            lock (DataBaseMock.Users)
            {
                var user = DataBaseMock.Users.SingleOrDefault(u => u.Login.Equals(player));
                if (user != null)
                    return user.LoggedIn;
                return false;
            }
        }

        private void AddPlayer(string xml, TcpClient tcpClient)
        {
            Client client = null;
            Client playerInRoom = null;

            client = FramesFactory.CreateObject<Client>(xml);

            if (!IsLogged(client.Login)) return;

            var posision = new Posision(client.Lat, client.Lng);

            lock (Server.Room.Players)
            {
                if (!Server.Room.Players.Contains(client))
                {
                    Server.Room.Players.Add(client);
                    client.PropertyChanged += Server.ClientPropertyChanged;
                    client.Connection = tcpClient;
                    client.Posision = posision; // Notify property changed
                    playerInRoom = client;

                    Server.OnMessageChange(new MessageEventArgs { Message = $"New player {client.Login}\n" });
                }
                else
                {
                    playerInRoom = (Client)Server.Room.Players.Single(p => p.Login.Equals(client.Login));
                    playerInRoom.Accuracy = client.Accuracy;
                    playerInRoom.Direction = client.Direction;
                    playerInRoom.Posision = posision;
                    playerInRoom.Message = client.Message;
                }

                Stats.StatsHandler.Update(client);
                playerInRoom.NotifyPropertyChanged();

                Server.OnContainerChange(new ContainerEventArgs
                {
                    Message = new Message()
                    {
                        User = playerInRoom.Login,
                        Adress = tcpClient.Client.RemoteEndPoint.ToString(),
                        Time = DateTime.Now,
                        RecivedData = client.Message
                    }
                });

                lock (Server.Container.RecivedMessages)
                     if (Server.Container.RecivedMessages.Count > 17)
                        Server.OnContainerChange(new ContainerEventArgs { Clean = true });
            }
        }
    }
}
