using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Server.Frames.Client
{
    public class SystemUser : IFrame, INotifyPropertyChanged
    {
        private bool _logggedIn;

        public SystemUser()
        {
            FrameType = Frames.Login;
        }

        public string Password { get; set; }

        public bool LoggedIn
        {
            get { return _logggedIn; }
            set
            {
                _logggedIn = value;
                NotifyPropertyChanged();
            }
        }

        public Frames FrameType { get; set; }

        public string Login { get; set; }

        [XmlIgnore]
        public TcpClient Connection { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
