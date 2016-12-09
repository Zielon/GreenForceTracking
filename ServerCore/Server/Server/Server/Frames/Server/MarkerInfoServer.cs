using Library.Common;
using System.Net.Sockets;
using System.Xml.Serialization;

namespace Library.Frames.Server
{
    public class MarkerInfoServer : IFrame
    {
        public Frames FrameType { get; set; }

        [XmlIgnoreAttribute]
        public string Login { get; set; }

        [XmlIgnoreAttribute]
        public TcpClient Connection { get; set; }

        public Marker Marker { get; set; }

        public MarkerInfoServer() { FrameType = Frames.Marker; }
    }
}
