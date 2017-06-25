using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Server.Frames;

namespace Server.Common
{
    /// <summary>
    ///     Implementaion of Observer design pattern
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class Client : INotifyPropertyChanged, IFrame
    {
        [XmlIgnore] private string _message = string.Empty;

        [XmlIgnore] private Posision _posision;

        public double Lat { get; set; }

        public double Lng { get; set; }

        public double Accuracy { get; set; }

        public double Direction { get; set; }

        [XmlIgnore]
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

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public string Login { get; set; }

        [XmlIgnore]
        public TcpClient Connection { get; set; }

        public Frames.Frames FrameType { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            var client = obj as Client;
            if (client != null) return client.Login.Equals(Login);
            return false;
        }

        public override int GetHashCode()
        {
            return Login.GetHashCode();
        }
    }
}
