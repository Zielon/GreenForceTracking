using System;

namespace Server.Events
{
    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}
