using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Xml;
using Server.Events;
using Server.Mock;

namespace Server.API
{
    public partial class MessagesHandler
    {
        private readonly Dictionary<Frames.Frames, Action<string, TcpClient>> _actions =
            new Dictionary<Frames.Frames, Action<string, TcpClient>>();

        private readonly Server _server;

        public MessagesHandler(Server server)
        {
            _server = server;
            _actions[Frames.Frames.Player] = AddPlayer;
            _actions[Frames.Frames.Login] = CheckLogin;
            _actions[Frames.Frames.Marker] = AddMarker;
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

                        _actions[type](xml, tcpClient);
                    }
            }
            catch (Exception ex) { _server.OnMessageChange(new MessageEventArgs { Message = ex.Message + "\n" + ex.StackTrace }); }
        }

        private bool IsLogged(string player)
        {
            lock (DataBaseMock.Users)
            {
                var user = DataBaseMock.Users.SingleOrDefault(u => u.Login.Equals(player));
                return user != null && user.LoggedIn;
            }
        }
    }
}
