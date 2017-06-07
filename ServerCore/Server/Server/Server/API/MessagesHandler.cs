﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Xml;
using Server.Common;
using Server.Events;
using Server.Frames;
using Server.Frames.Client;
using Server.Messages;
using Server.Mock;
using Server.Stats;

namespace Server.API
{
    public class MessagesHandler
    {
        private readonly Server Server;

        public MessagesHandler(Server server)
        {
            Server = server;
        }

        public void ParseMessage(string msg, TcpClient tcpClient)
        {
            XmlDocument doc = new XmlDocument();

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
                                AddPlayer(xml, tcpClient);
                                break;
                            case Frames.Frames.Login:
                                CheckLogin(xml, tcpClient);
                                break;
                            case Frames.Frames.Marker:
                                AddMarker(xml, tcpClient);
                                break;
                            default: throw new InvalidEnumArgumentException();
                        }
                    }
            }
            catch (Exception ex) { Server.OnMessageChange(new MessageEventArgs { Message = ex.Message + "\n" + ex.StackTrace }); }
        }

        private void AddMarker(string xml, TcpClient tcpClient)
        {
            Marker marker = FramesFactory.CreateObject<Marker>(xml);

            if (!IsLogged(marker.Login)) return;

            StatsHandler.AddMarker(marker.Login);

            lock (Server.Room.Markers)
            {
                if (!Server.Room.Markers.Contains(marker) && marker.Add)
                {
                    Server.Room.Markers.Add(marker);
                    marker.PropertyChanged += Server.ClientPropertyChanged;
                    marker.Connection = tcpClient;
                    marker.NotifyPropertyChanged();

                    Server.OnMessageChange(new MessageEventArgs { Message = $"Marker has been added by {marker.Login} !\n" });
                    lock (Server.Container.RecivedMessages)
                    {
                        if (Server.Container.RecivedMessages.Count > 17) Server.OnContainerChange(new ContainerEventArgs { Clean = true });
                    }
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

                user.PropertyChanged += Server.ClientPropertyChanged;
                user.Connection = tcpClient;
                user.LoggedIn = true;
            }
        }

        private bool IsLogged(string player)
        {
            lock (DataBaseMock.Users)
            {
                var user = DataBaseMock.Users.SingleOrDefault(u => u.Login.Equals(player));
                return user != null && user.LoggedIn;
            }
        }

        private void AddPlayer(string xml, TcpClient tcpClient)
        {
            Client client = null;

            client = FramesFactory.CreateObject<Client>(xml);

            if (!IsLogged(client.Login)) return;

            var posision = new Posision(client.Lat, client.Lng);

            lock (Server.Room.Players)
            {
                Client playerInRoom = null;
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
                    playerInRoom = (Client) Server.Room.Players.Single(p => p.Login.Equals(client.Login));
                    playerInRoom.Accuracy = client.Accuracy;
                    playerInRoom.Direction = client.Direction;
                    playerInRoom.Posision = posision;
                    playerInRoom.Message = client.Message;
                }

                StatsHandler.Update(client);
                playerInRoom.NotifyPropertyChanged();

                Server.OnContainerChange(
                    new ContainerEventArgs
                    {
                        Message = new Message
                        {
                            User = playerInRoom.Login,
                            Adress = tcpClient.Client.RemoteEndPoint.ToString(),
                            Time = DateTime.Now,
                            RecivedData = client.Message
                        }
                    });

                lock (Server.Container.RecivedMessages)
                {
                    if (Server.Container.RecivedMessages.Count > 17) Server.OnContainerChange(new ContainerEventArgs { Clean = true });
                }
            }
        }
    }
}
