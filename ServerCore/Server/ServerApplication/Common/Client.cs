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

        public string ID { get; set; }

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
                return client.ID.Equals(ID);
            else return false;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }


        //Temporary solution
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("<Player>");
            builder.Append(string.Format("<ID>{0}</ID>", ID));
            builder.Append(string.Format("<User>{0}</User>", UserName));
            builder.Append(string.Format("<Lat>{0}</Lat>", Lat));
            builder.Append(string.Format("<Lon>{0}</Lon>", Lon));
            builder.Append(string.Format("<Message>{0}</Message>", Message));
            builder.Append("</Player>");

            return builder.ToString();
        }
    }
}
