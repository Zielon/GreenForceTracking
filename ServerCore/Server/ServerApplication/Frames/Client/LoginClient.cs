using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication.Frames.Client
{
    public class LoginClient : IFrame
    {
        public Frames FrameType { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public LoginClient(string login, string password, Frames type) {
            FrameType = type;
            Login = login;
            Password = password;
        }

        public LoginClient() { }
    }
}
