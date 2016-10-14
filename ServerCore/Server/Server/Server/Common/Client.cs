using Library.Frames;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Library.Common
{
    /// <summary>
    /// Implementaion of Observer design pattern
    /// </summary>
    public class Client : INotifyPropertyChanged, IFrame
    {

        public string ID { get; set; }

        public string UserName { get; set; }

        public string RoomId { get; set; }

        public double Lat { get; set; }

        public double Lon { get; set; }


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
                Lon = _posision.Lon;
                NotifyPropertyChanged(); //Notify posistion update
            }
        }

        public string Message { get { return _message; } set { _message = value; } }

        public Frames.Frames FrameType { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
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
