using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication.Common
{
    public class Room
    {
        public ObservableCollection<Client> Players { get; set; }
        public Room()
        {
            Players = new ObservableCollection<Client>();
        }
    }
}
