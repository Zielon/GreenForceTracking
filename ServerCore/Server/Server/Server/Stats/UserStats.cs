using System;
using System.Collections.Generic;

namespace Server.Stats
{
    public class StatsPerUser
    {
        public double MessagesPerSecond { get; set; }
        public double Kbps { get; set; }
        public double Accuracy { get; set; }
        public string Name { get; set; }
        public double Count { get; set; }
        public double Kb { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double Speed { get; set; }
        public double Distance { get; set; }
        public DateTime Start { get; set; }
        public int Markers { get; set; }
        public string IP { get; set; }

        public override string ToString()
        {
            var duration = DateTime.Now - Start;
            var strings = new List<string>
            {
                string.Format("{0,-20} | {1,-20}","User", Name ),
                string.Format("{0,-20} | {1,-20}","IP:", IP ),
                string.Format("{0,-20} | {1,-20}","GPS accuracy", $"{Accuracy.ToString("0.##")} m"),
                string.Format("{0,-20} | {1,-20}","Internet speed", $"{Kbps.ToString("0.##")} kbps"),
                string.Format("{0,-20} | {1,-20}","Kb in total",$"{Kb.ToString("0.##")}"),
                string.Format("{0,-20} | {1,-20}","Frames per second", $"{MessagesPerSecond.ToString("0.##")}" ),
                string.Format("{0,-20} | {1,-20}","Frames in total",$"{Count}" ),
                string.Format("{0,-20} | {1,-20}","Start",$"{Start.ToString("yyyy-MM-dd [HH:mm:ss]")}" ),
                string.Format("{0,-20} | {1,-20}","Duration", $"{duration.ToString(@"hh\:mm\:ss\.fff")}" ),
                string.Format("{0,-20} | {1,-20}","Actual latitude",$"{Lat.ToString("0.########")}" ),
                string.Format("{0,-20} | {1,-20}","Actual longitude:",$"{Lon.ToString("0.########")}" ),
                string.Format("{0,-20} | {1,-20}","Avrage speed",$"{Speed.ToString("0.##")} km\\h" ),
                string.Format("{0,-20} | {1,-20}","Distance",$"{Distance.ToString("0.##")} m" ),
                string.Format("{0,-20} | {1,-20}","Markers",$"{Markers}" ),
            };
            return string.Join("\n", strings);
        }
    }
}
