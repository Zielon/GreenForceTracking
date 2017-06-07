using System.Net.Sockets;
using System.Xml.Serialization;

namespace Library.Frames.Server
{
    public class RoomInfoServer : IFrame
    {
        public RoomInfoServer()
        {
            FrameType = Frames.RoomInfo;
        }

        public Common.Client Client { get; set; }
        public Frames FrameType { get; set; }

        [XmlIgnore]
        public string Login { get; set; }

        [XmlIgnore]
        public TcpClient Connection { get; set; }
    }
}
