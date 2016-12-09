using Library.Frames;
using System.Collections.ObjectModel;

namespace Library.Common
{
    public class Room
    {
        public ObservableCollection<IFrame> Players { get; set; }
        public ObservableCollection<IFrame> Markers { get; set; }
        public Room()
        {
            Players = new ObservableCollection<IFrame>();
            Markers = new ObservableCollection<IFrame>();
        }
    }
}
