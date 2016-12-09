﻿using Library.Frames;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Library.Common
{
    public class Marker : INotifyPropertyChanged, IFrame
    {
        public string Id { get; set; }

        public string Text { get; set; }

        public double Lat { get; set; }

        public double Lng { get; set; }

        public bool Add { get; set; }

        public string Login { get; set; }

        [XmlIgnoreAttribute]
        public TcpClient Connection { get; set; }

        public Frames.Frames FrameType { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            var marker = obj as Marker;
            if (marker != null)
                return marker.Id.Equals(Id);
            else return false;
        }

        public Marker() { }
    }
}
