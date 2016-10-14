using Library;
using Library.Messages;
using System;

namespace Library.Events
{
    public class ContainerEventArgs : EventArgs
    {
        public Message Message { get; set; }
        public bool Clean { get; set; }
    }
}
