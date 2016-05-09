using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication
{
    public class ClientMessage
    {
        public IPAddress Adress { get; set; }
        public string RecivedData { get; set; }
        public DateTime Time { get; set; }
    }
}
