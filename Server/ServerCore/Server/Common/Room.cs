using System.Collections.ObjectModel;
using Server.Frames;

namespace Server.Common
{
    public class Room
    {
        public Room()
        {
            Players = new ObservableCollection<IFrame>();
            Markers = new ObservableCollection<IFrame>();
        }

        public ObservableCollection<IFrame> Players { get; set; }
        public ObservableCollection<IFrame> Markers { get; set; }
    }
}
