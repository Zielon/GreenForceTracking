using System.Net.Sockets;
using System.Xml.Serialization;
using Library.Common;

namespace Library.Frames.Server
{
    public class MarkerInfoServer : IFrame
    {
        public MarkerInfoServer()
        {
            FrameType = Frames.Marker;
        }

        public Marker Marker { get; set; }
        public Frames FrameType { get; set; }

        [XmlIgnore]
        public string Login { get; set; }

        [XmlIgnore]
        public TcpClient Connection { get; set; }
    }
}
