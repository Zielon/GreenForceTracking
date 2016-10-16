using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Library.Frames.Client
{
    public class SystemUser : IFrame, INotifyPropertyChanged
    {
        private bool _logggedIn;

        public Frames FrameType { get; set; }

        public string Password { get; set; }

        public bool LoggedIn
        {
            get { return _logggedIn; }
            set
            {
                _logggedIn = value;
                if (value)
                    NotifyPropertyChanged();
            }
        }

        public string Login{ get; set; }

        public SystemUser()
        {
            FrameType = Frames.Login;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [XmlIgnoreAttribute]
        public TcpClient Connection { get; set; }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
