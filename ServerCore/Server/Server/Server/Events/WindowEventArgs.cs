using System;

namespace Server.Events
{
    public class WindowEventArgs : EventArgs
    {
        public bool ChangeBrush { get; set; }
        public string Running { get; set; }
    }
}
