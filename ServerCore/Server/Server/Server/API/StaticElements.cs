using Library.Common;
using Library.Frames;
using Library.Frames.Server;

namespace Server.API
{
    public class StaticElements
    {
        public static RoomInfoServer Broadcast = new RoomInfoServer()
        {
            Client = new Client
            {
                Message = "Broadcasting...",
                Lat = -1,
                Lng = -1,
                Accuracy = -1,
                Login = "SERVER",
                FrameType = Frames.RoomInfo
            },
            Login = "SERVER"
        };
    }
}
