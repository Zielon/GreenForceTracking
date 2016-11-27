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
                                AddPlayerToTheRoom(xml, tcpClient);
                                break;
                            case Frames.Login:
                                CheckLogin(FramesFactory.CreateObject<SystemUser>(xml), tcpClient);
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

        private void CheckLogin(SystemUser client, TcpClient tcpClient)
        {
            lock (DataBaseMock.Users)
            {
                var user = DataBaseMock.Users.SingleOrDefault(u => u.Password.Equals(client.Password) && u.Login.Equals(client.Login));
                if (user == null) { user.LoggedIn = false; return; }
                if (user != null)
                {
                    user.PropertyChanged += Server.ClientPropertyChanged;
                    user.Connection = tcpClient;
                    user.LoggedIn = true;
                    return;
                }
            }
        }

        private bool IsLogged(Client player)
        {
            lock (DataBaseMock.Users)
            {
                var user = DataBaseMock.Users.SingleOrDefault(u => u.Login.Equals(player.Login));
                if (user != null)
                    return user.LoggedIn;
                return false;
            }
        }

        private void AddPlayerToTheRoom(string xml, TcpClient tcpClient)
        {
            Client client = null;
            Client playerInTheRoom = null;

            client = FramesFactory.CreateObject<Client>(xml);

            if (!IsLogged(client)) return;

            var posision = new Posision(client.Lat, client.Lon);

            lock (Server.Room.Players)
            {
                if (!Server.Room.Players.Contains(client))
                {
                    Server.Room.Players.Add(client);
                    client.PropertyChanged += Server.ClientPropertyChanged;
                    client.Connection = tcpClient;
                    client.Accuracy = client.Accuracy;
                    client.Posision = posision; // Notify property changed
                    playerInTheRoom = client;

                    Server.OnMessageChange(new MessageEventArgs { Message = $"New player {client.Login}\n" });
                }
                else
                {
                    playerInTheRoom = Server.Room.Players.Single(p => p.Login.Equals(client.Login));
                    if (!playerInTheRoom.Posision.Equals(posision))
                        playerInTheRoom.Posision = posision; // Notify property changed
                    playerInTheRoom.Message = client.Message;
                    playerInTheRoom.Accuracy = client.Accuracy;
                }
            }

            Server.OnContainerChange(new ContainerEventArgs
            {
                Message = new Message()
                {
                    User = playerInTheRoom.Login,
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
