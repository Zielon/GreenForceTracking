using System;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ServerApplication
{
    public class Server
    {
        private IPAddress ipAddress;
        private int port;
        private MainWindow window;
        public static bool isRunning = false;

        public MessagesContainer container = new MessagesContainer();

        public Server(string ipAddress, int port, MainWindow mainwindow)
        {
            this.ipAddress = IPAddress.Parse(ipAddress);
            this.port = port;
            this.window = mainwindow;
            window.dataGrid.DataContext = container.RecivedMessages;
        }

        // TODO start sending data to clients
        public async void StartSending()
        {

        }

        public async void StartListening()
        {
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(this.ipAddress, this.port);
                listener.Start();
                isRunning = true;
                window.ServerStatus.Content = "Server is running";
                window.ServerStatus.Foreground = new SolidColorBrush(Colors.Green);

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

        private async Task Process(TcpClient tcpClient)
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
                        string recivedData = ParseMessage(message);
                        string clientEndPoint = tcpClient.Client.RemoteEndPoint.ToString().Split(':').First();

                        container.RecivedMessages.Add(new Message()
                        {
                            Adress = IPAddress.Parse(clientEndPoint),
                            Time = DateTime.Now,
                            RecivedData = recivedData
                        });
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

        //TODO parse response
        private static string ParseMessage(string msg)
        {
            return msg;
        }
    }
}
