using System;

namespace Server.Messages
{
    public class Message
    {
        public DateTime Time { get; set; }
        public string User { get; set; }
        public string Adress { get; set; }
        public string RecivedData { get; set; }
    }
}
