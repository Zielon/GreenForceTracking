using System.Net.Sockets;
using System.Xml.Serialization;
using Library.Frames;

namespace Library.Common
{
    public class RemoveUser : IFrame
    {
        public Frames.Frames FrameType { get; set; }

        public string Login { get; set; }

        [XmlIgnore]
        public TcpClient Connection { get; set; }
    }
}
