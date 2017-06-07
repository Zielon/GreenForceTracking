using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Library.Common;

namespace Server.Stats
{
    public static class StatsHandler
    {
        public static Dictionary<string, StatsPerUser> StatsCollection = new Dictionary<string, StatsPerUser>();

        public static void AddMarker(string user)
        {
            lock (StatsCollection)
            {
                if (StatsCollection.ContainsKey(user))
                {
                    var stats = StatsCollection[user];
                    stats.Markers += 1;
                }
            }
        }

        private static double distance(double lat1, double lon1, double lat2, double lon2, char unit)
        {
            double theta = lon1 - lon2;
            double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2))
                          + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
            dist = Math.Acos(dist);
            dist = rad2deg(dist);
            dist = dist * 60.0 * 1.1515;
            if (unit == 'K') dist = dist * 1.609344;
            else if (unit == 'N') dist = dist * 0.8684;
            return dist;
        }

        private static double deg2rad(double deg)
        {
            return deg * Math.PI / 180.0;
        }

        private static double rad2deg(double rad)
        {
            return rad / Math.PI * 180.0;
        }

        public static void Update(Client user)
        {
            lock (StatsCollection)
            {
                long bytes = Marshal.SizeOf(typeof(Client));

                if (StatsCollection.ContainsKey(user.Login))
                {
                    var stats = StatsCollection[user.Login];
                    double dist = distance(stats.Lat, stats.Lon, user.Lat, user.Lng, 'K');

                    stats.Count += 1;
                    stats.Kb += bytes / 1000.0;
                    stats.Accuracy = user.Accuracy;
                    stats.MessagesPerSecond = stats.Count / (DateTime.Now - stats.Start).Seconds;
                    stats.Kbps = stats.Kb / (DateTime.Now - stats.Start).Seconds;
                    stats.Lon = user.Lng;
                    stats.Lat = user.Lat;
                    stats.Distance += dist * 1000.0;
                    double speed_kph = stats.Distance / 1000.0 / (DateTime.Now - stats.Start).Seconds * 3600.0;
                    stats.Speed = speed_kph;
                }
                else
                {
                    var stats = new StatsPerUser
                    {
                        Count = 1,
                        Accuracy = user.Accuracy,
                        MessagesPerSecond = 1.0,
                        Kbps = bytes / 1000.0,
                        Kb = bytes / 1000.0,
                        Start = DateTime.Now,
                        Name = user.Login,
                        Markers = 0,
                        Lat = user.Lat,
                        Lon = user.Lng,
                        Speed = 0.0,
                        Distance = 0.0,
                        IP = user.Connection.Client.RemoteEndPoint.ToString()
                    };

                    StatsCollection[user.Login] = stats;
                }
            }
        }
    }
}
