using System;
using System.Linq;
using System.Net.Sockets;
using Server.Common;
using Server.Events;
using Server.Frames;
using Server.Frames.Client;
using Server.Messages;
using Server.Mock;
using Server.Stats;

namespace Server.API
{
    public partial class MessagesHandler
    {
        private void AddMarker(string xml, TcpClient tcpClient)
        {
            Marker marker = FramesFactory.CreateObject<Marker>(xml);

            if (!IsLogged(marker.Login)) return;

            StatsHandler.AddMarker(marker.Login);

            lock (_server.Room.Markers)
            {
                if (!_server.Room.Markers.Contains(marker) && marker.Add)
                {
                    _server.Room.Markers.Add(marker);
                    marker.PropertyChanged += _server.ClientPropertyChanged;
                    marker.Connection = tcpClient;
                    marker.NotifyPropertyChanged();

                    _server.OnMessageChange(new MessageEventArgs { Message = $"Marker has been added by {marker.Login} !\n" });
                    lock (_server.Container.RecivedMessages)
                    {
                        if (_server.Container.RecivedMessages.Count > 17) _server.OnContainerChange(new ContainerEventArgs { Clean = true });
                    }
                }
                else if (_server.Room.Markers.Contains(marker) && !marker.Add)
                {
                    Marker m = _server.Room.Markers.Cast<Marker>().Single(e => e.Id == marker.Id);
                    m.Add = false;
                    m.NotifyPropertyChanged();
                    _server.Room.Markers.Remove(m);
                    _server.OnMessageChange(new MessageEventArgs { Message = $"Marker {m.Id} has been deleted !\n" });
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
                    client.PropertyChanged += _server.ClientPropertyChanged;
                    client.Connection = tcpClient;
                    client.LoggedIn = false;
                    return;
                }

                user.PropertyChanged += _server.ClientPropertyChanged;
                user.Connection = tcpClient;
                user.LoggedIn = true;
            }
        }

        private void AddPlayer(string xml, TcpClient tcpClient)
        {
            Client client = null;

            client = FramesFactory.CreateObject<Client>(xml);

            if (!IsLogged(client.Login)) return;

            var posision = new Posision(client.Lat, client.Lng);

            lock (_server.Room.Players)
            {
                Client playerInRoom = null;
                if (!_server.Room.Players.Contains(client))
                {
                    _server.Room.Players.Add(client);
                    client.PropertyChanged += _server.ClientPropertyChanged;
                    client.Connection = tcpClient;
                    client.Posision = posision; // Notify property changed
                    playerInRoom = client;

                    _server.OnMessageChange(new MessageEventArgs { Message = $"New player {client.Login}\n" });
                }
                else
                {
                    playerInRoom = (Client) _server.Room.Players.Single(p => p.Login.Equals(client.Login));
                    playerInRoom.Accuracy = client.Accuracy;
                    playerInRoom.Direction = client.Direction;
                    playerInRoom.Posision = posision;
                    playerInRoom.Message = client.Message;
                }

                StatsHandler.Update(client);
                playerInRoom.NotifyPropertyChanged();

                _server.OnContainerChange(
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

                lock (_server.Container.RecivedMessages)
                {
                    if (_server.Container.RecivedMessages.Count > 17) _server.OnContainerChange(new ContainerEventArgs { Clean = true });
                }
            }
        }
    }
}
