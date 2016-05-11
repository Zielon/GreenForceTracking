using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication
{
    class Client
    {
        public string HashCode { get; set; }

        public string UserName { get; set; }

        public IPAddress IpAddress { get; set; }

        public double Lat { get; set; }

        public double Lon { get; set; }

        public ChatMessage Message { get; set; }

    }
}
