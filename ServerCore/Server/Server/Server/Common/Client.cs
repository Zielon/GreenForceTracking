using Library.Frames;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace Library.Common
{
    /// <summary>
    /// Implementaion of Observer design pattern
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class Client : INotifyPropertyChanged, IFrame
    {
        public string Login { get; set; }

        public double Lat { get; set; }

        public double Lng { get; set; }

        public double Accuracy { get; set; }

        public double Direction { get; set; }

        [XmlIgnoreAttribute]
        public TcpClient Connection { get; set; }

        [XmlIgnoreAttribute]
        private string _message = string.Empty;

        [XmlIgnoreAttribute]
        private Posision _posision;

        [XmlIgnoreAttribute]
        public Posision Posision
        {
            get { return _posision; }
            set
            {
                _posision = value;
                Lat = _posision.Lat;
                Lng = _posision.Lon;
            }
        }

        public string Message {
            get { return _message; }
            set {
                _message = value;
            }
        }

        public Frames.Frames FrameType { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            var client = obj as Client;
            if (client != null)
                return client.Login.Equals(Login);
            else return false;
        }

        public override int GetHashCode()
        {
            return Login.GetHashCode();
        }

        public Client() { }
    }
}
