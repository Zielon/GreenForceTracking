using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Library.Frames;

namespace Library.Common
{
    public class Marker : INotifyPropertyChanged, IFrame
    {
        public string Id { get; set; }

        public string Text { get; set; }

        public List<Posision> Points { get; set; }

        public bool Add { get; set; }

        public bool Outside { get; set; }

        public int Color { get; set; }

        public string Type { get; set; }

        public string Login { get; set; }

        public Frames.Frames FrameType { get; set; }

        [XmlIgnore]
        public TcpClient Connection { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            var marker = obj as Marker;
            if (marker != null) return marker.Id.Equals(Id);
            return false;
        }
    }
}
