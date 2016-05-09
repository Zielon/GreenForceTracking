using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication
{
    public class Server
    {
        private IPAddress ipAddress;
        private int port;
        private MainWindow window;
        public static bool isRunning = false;

        private ObservableCollection<ClientMessage> recivedMessages = 
            new ObservableCollection<ClientMessage>() {
                new ClientMessage() {Time = DateTime.Now, Adress = IPAddress.Parse("123.123.123.123"), RecivedData = "Afaf" } };

        public ObservableCollection<ClientMessage> RecivedMessages { get { return recivedMessages; } }

        public Server(string ipAddress, int port, MainWindow mainwindow)
        {
            this.ipAddress = IPAddress.Parse(ipAddress);
            this.port = port;
            this.window = mainwindow;
            window.dataGrid.ItemsSource = recivedMessages;
        }

        public async void Run()
        {
            TcpListener listener = null;
            isRunning = true;
            try
            {
                listener = new TcpListener(this.ipAddress, this.port);
                listener.Start();

                window.ServerStatus.Content = "Server is running";

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
                    string request = await reader.ReadLineAsync();

                    if (request != null)
                    {
                        string recivedData = Response(request);
                        string clientEndPoint = tcpClient.Client.RemoteEndPoint.AddressFamily.ToString();

                        recivedMessages.Add(new ClientMessage()
                        {
                            Adress = IPAddress.Parse(clientEndPoint),
                            Time = DateTime.Now,
                            RecivedData = recivedData
                        });

                       
                    }
                    else
                        break; // Client closed connection
                }
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                if (tcpClient.Connected) tcpClient.Close();
            }
        }

        private static string Response(string request)
        {
            return request;
        }
    }
}
