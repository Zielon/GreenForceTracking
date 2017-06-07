using System.Xml.Serialization;

namespace Server.Frames
{
    public enum Frames
    {
        [XmlEnum(Name = "RemovingUser")] RemovingUser,

        [XmlEnum(Name = "Login")] Login,

        [XmlEnum(Name = "RoomInfo")] RoomInfo,

        [XmlEnum(Name = "Player")] Player,

        [XmlEnum(Name = "Marker")] Marker
    }
}
