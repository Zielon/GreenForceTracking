using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication
{
    /// <summary>
    /// Implementaion of Observer design pattern
    /// </summary>
    public class Client : INotifyPropertyChanged
    {
        private double _lat;
        private double _lon;
        private string _message = string.Empty;
        private Posision _posision;

        public string SessionID { get; set; }

        public string UserName { get; set; }

        public IPAddress IpAddress { get; set; }

        public double Lat {
            get { return _posision.Lat; }
            private set { _lat = value; }
        }

        public double Lon {
            get { return _posision.Lon; }
            private set { _lon = value; }
        }

        public Posision Posision
        {
            get { return _posision; }
            set {
                _posision = value;
                NotifyPropertyChanged(); //Notify posistion update
                _lat = _posision.Lat;
                _lon = _posision.Lon;
            }
        }

        public string Message { get { return _message; } set { _message = value; NotifyPropertyChanged(); } }

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
                return client.SessionID.Equals(SessionID);
            else return false;
        }

        public override int GetHashCode()
        {
            return SessionID.GetHashCode();
        }
    }
}
