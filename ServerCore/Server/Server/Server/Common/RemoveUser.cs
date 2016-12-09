using Library.Frames;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Xml.Serialization;

namespace Library.Common
{
    public class RemoveUser : IFrame
    {
        public Frames.Frames FrameType { get; set; }

        public string Login { get; set; }

        [XmlIgnoreAttribute]
        public TcpClient Connection { get; set; }
    }
}
