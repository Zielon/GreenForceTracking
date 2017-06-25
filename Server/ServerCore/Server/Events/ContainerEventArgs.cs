using System;
using Server.Messages;

namespace Server.Events
{
    public class ContainerEventArgs : EventArgs
    {
        public Message Message { get; set; }
        public bool Clean { get; set; }
    }
}
