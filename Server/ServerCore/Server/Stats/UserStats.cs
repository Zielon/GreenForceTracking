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
                $"{"User",-20} | {Name,-20}",
                $"{"IP:",-20} | {IP,-20}",
                $"{"GPS accuracy",-20} | {$"{Accuracy.ToString("0.##")} m",-20}",
                $"{"Internet speed",-20} | {$"{Kbps.ToString("0.##")} kbps",-20}",
                $"{"Kb in total",-20} | {$"{Kb.ToString("0.##")}",-20}",
                $"{"Frames per second",-20} | {$"{MessagesPerSecond.ToString("0.##")}",-20}",
                $"{"Frames in total",-20} | {$"{Count}",-20}",
                $"{"Start",-20} | {$"{Start.ToString("yyyy-MM-dd [HH:mm:ss]")}",-20}",
                $"{"Duration",-20} | {$"{duration.ToString(@"hh\:mm\:ss\.fff")}",-20}",
                $"{"Actual latitude",-20} | {$"{Lat.ToString("0.########")}",-20}",
                $"{"Actual longitude:",-20} | {$"{Lon.ToString("0.########")}",-20}",
                $"{"Avrage speed",-20} | {$"{Speed.ToString("0.##")} km\\h",-20}",
                $"{"Distance",-20} | {$"{Distance.ToString("0.##")} m",-20}",
                $"{"Markers",-20} | {$"{Markers}",-20}"
            };
            return string.Join("\n", strings);
        }
    }
}
