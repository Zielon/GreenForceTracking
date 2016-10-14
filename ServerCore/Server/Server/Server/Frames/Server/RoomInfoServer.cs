using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library.Common;

namespace Library.Frames.Server
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
