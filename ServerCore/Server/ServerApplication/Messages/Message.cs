﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication
{
    public class Message
    {
        public DateTime Time { get; set; }
        public string User { get; set; }
        public string Adress { get; set; }
        public string RecivedData { get; set; }
    }
}
