using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ServerApplication.Frames
{

    public enum Frames {

        [XmlEnum(Name = "Login")]
        Login,

        [XmlEnum(Name = "ListOfRooms")]
        ListOfRooms,

        [XmlEnum(Name = "Disconnect")]
        Disconnect,

        [XmlEnum(Name = "Chatmessage")]
        Chatmessage,

        [XmlEnum(Name = "RoomInfo")]
        RoomInfo
    }
}
