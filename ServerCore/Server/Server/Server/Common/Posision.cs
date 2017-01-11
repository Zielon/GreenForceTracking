﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Common
{
    public sealed class Posision
    {
        public double Lat { get; set; }
        public double Lon { get; set; }

        public Posision(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }
        public Posision() { }

        public override bool Equals(object obj)
        {
            var pos = obj as Posision;
            if (pos != null && pos.Lat == this.Lat && pos.Lon == this.Lon)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Lat: {0}, Lon: {1}", Lat, Lon);
        }
    }
}
