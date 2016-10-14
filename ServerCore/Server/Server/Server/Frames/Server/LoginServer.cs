using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;
using System.Xml.Serialization;


namespace Library.Frames.Server
{
    public class LoginServer : IFrame
    {
        public Frames FrameType { get; set; }

        public int Status { get; set; }

        public string SessionId { get; set; }

        public LoginServer() {
            SessionId = GetHashCode().ToString();
            Status = -1;
        }
    }
}