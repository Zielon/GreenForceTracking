using System.Xml.Serialization;

namespace Library.Frames
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
        RoomInfo,

        [XmlEnum(Name = "Player")]
        Player
    }
}
