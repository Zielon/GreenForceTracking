using ServerApplication.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication.Frames.Server
{
    public class ListOfRoomsServer : IFrame
    {
        public Frames FrameType { get; set; }

        public Dictionary<string, Room> Rooms { get; set; }
    }
}
