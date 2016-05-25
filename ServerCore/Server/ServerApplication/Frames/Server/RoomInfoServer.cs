using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerApplication.Common;

namespace ServerApplication.Frames.Server
{
    public class RoomInfoServer : IFrame
    {
        public Frames FrameType { get; set; }

        public List<Common.Client> Players { get; set; }

        public RoomInfoServer() {
            FrameType = Frames.RoomInfo;
        }
    }
}
