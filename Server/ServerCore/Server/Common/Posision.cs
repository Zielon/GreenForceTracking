namespace Server.Common
{
    public sealed class Posision
    {
        public Posision(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }

        public Posision() { }
        public double Lat { get; set; }
        public double Lon { get; set; }

        public override bool Equals(object obj)
        {
            var pos = obj as Posision;
            if (pos != null && pos.Lat == Lat && pos.Lon == Lon) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"Lat: {Lat}, Lon: {Lon}";
        }
    }
}
