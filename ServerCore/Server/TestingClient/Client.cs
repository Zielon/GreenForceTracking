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
        public static MainWindow window;
        public static IPAddress Ip;
        public static int Port;
        public static bool isConnedted = false;
        private static int count;

        public static async void StartListening()
        {
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(IPAddress.Parse("192.168.0.2"), 52300);
                listener.Start();

                while (true)
                {
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                    Task t = Process(tcpClient);
                    await t;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static async Task Process(TcpClient tcpClient)
        {
            try
            {
                NetworkStream networkStream = tcpClient.GetStream();
                StreamReader reader = new StreamReader(networkStream);

                while (true)
                {
                    string message = await reader.ReadLineAsync();

                    if (message != null)
                    {
                        window.textBoxResponse.Text = message;
                    }
                    else
                        break; // Closed connection
                }

                tcpClient.Close();
            }
            catch (Exception ex)
            {
                if (tcpClient.Connected) tcpClient.Close();
            }
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
               
                await writer.WriteLineAsync(@"<Player><ID>1234</ID><User>Tesownik</User><Lat>123.234</Lat><Lon>123.234</Lon><Message>Test</Message></Player>");
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
