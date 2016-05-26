using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication.Common
{
    public sealed class Posision
    {
        public double Lat { get; private set; }
        public double Lon { get; private set; }

        public Posision(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }

        public override bool Equals(object obj)
        {
            var pos = obj as Posision;
            if (pos != null && pos.Lat == this.Lat && pos.Lon == this.Lon)
                return true;
            return false;
        }

        public override string ToString()
        {
            return string.Format("Lat: {0}, Lon: {1}", Lat, Lon);
        }
    }
}
