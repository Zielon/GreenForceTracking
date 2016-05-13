using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication
{
    public class Client
    {
        public string ID { get; set; }

        public string UserName { get; set; }

        public IPAddress IpAddress { get; set; }

        public double Lat { get; set; }

        public double Lon { get; set; }

        public string Message { get; set; }

        public override bool Equals(object obj)
        {
            var client = obj as Client;
            if (client != null)
                return client.ID.Equals(ID);
            else return false;
        }

        public override int GetHashCode()
        {
            return  ID.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("<Player>");
            builder.Append(string.Format("<ID>{0}</ID>", ID));
            builder.Append(string.Format("<User>{0}</User>", UserName));
            builder.Append(string.Format("<Lat>{0}</Lat>", Lat));
            builder.Append(string.Format("<Lon>{0}</Lon>", Lon));
            builder.Append("</Player>");

            return builder.ToString();
        }
    }
}
