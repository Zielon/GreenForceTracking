using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestingClient
{
    class Client
    {
        MainWindow window;
        public static IPAddress Ip;
        public static int Port;
        public static bool isConnedted = false;
        private static int count;

        public Client(MainWindow w)
        {
            this.window = w;
        }

        public static async Task Send(string data)
        {

            if (!isConnedted) return;

            try
            {
                TcpClient client = new TcpClient();

                await client.ConnectAsync(Ip, Port);

                NetworkStream networkStream = client.GetStream();
                StreamWriter writer = new StreamWriter(networkStream);
                writer.AutoFlush = true;
               
                await writer.WriteLineAsync(data += " " + count);
                count++;
                client.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }                  
    }
}
