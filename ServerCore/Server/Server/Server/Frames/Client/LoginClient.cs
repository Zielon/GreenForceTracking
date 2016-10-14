using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Frames.Client
{
    public class LoginClient : IFrame
    {
        public Frames FrameType { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public LoginClient() {
            FrameType = Frames.Login;
        }
    }
}
