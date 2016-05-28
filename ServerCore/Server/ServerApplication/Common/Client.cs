using ServerApplication.Frames;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ServerApplication.Common
{
    /// <summary>
    /// Implementaion of Observer design pattern
    /// </summary>
    public class Client : INotifyPropertyChanged, IFrame
    {
        [XmlIgnoreAttribute]
        private string _message = string.Empty;

        [XmlIgnoreAttribute]
        private Posision _posision;

        public string ID { get; set; }

        public string UserName { get; set; }

        [XmlIgnoreAttribute]
        public IPAddress IpAddress { get; set; }

        public string RoomId { get; set; }

        public double Lat { get; set; }

        public double Lon { get; set; }

        [XmlIgnoreAttribute]
        public Posision Posision
        {
            get { return _posision; }
            set {
                _posision = value;
                NotifyPropertyChanged(); //Notify posistion update
                Lat = _posision.Lat;
                Lon = _posision.Lon;
            }
        }

        public string Message { get { return _message; } set { _message = value; } }

        public Frames.Frames FrameType { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            var client = obj as Client;
            if (client != null)
                return client.UserName.Equals(UserName);
            else return false;
        }

        public override int GetHashCode()
        {
            return UserName.GetHashCode();
        }

        public Client() { }
    }
}
