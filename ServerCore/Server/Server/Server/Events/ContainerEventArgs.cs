using System;
using Library.Messages;

namespace Library.Events
{
    public class ContainerEventArgs : EventArgs
    {
        public Message Message { get; set; }
        public bool Clean { get; set; }
    }
}
