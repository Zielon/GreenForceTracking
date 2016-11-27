using System.Collections.Generic;
using System.Xml.Serialization;

namespace Library.Frames.Server
{
    public class RoomInfoServer : IFrame
    {
        public Frames FrameType { get; set; }

        [XmlIgnoreAttribute]
        public string Login { get; set; }

        public Common.Client Client { get; set; }

        public RoomInfoServer() { FrameType = Frames.RoomInfo; }
    }
}
