using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ServerApplication.Frames.Server
{
    public class LoginServer : IFrame
    {
        public Frames FrameType { get; set; }

        public int Status { get; set; }

        public string SessionId { get; set; }
    }
}