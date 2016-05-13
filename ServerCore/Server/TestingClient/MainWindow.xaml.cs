using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestingClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            Client.window = this;
        }

        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            Task tsResponse = Client.Send(this.textBox.Text);          
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var dns = textBoxIP.Text;
            var port = int.Parse(textBoxPort.Text);

            IPHostEntry ipHostInfo = Dns.GetHostEntry(dns);
            for (int i = 0; i < ipHostInfo.AddressList.Length; ++i)
            {
                if (ipHostInfo.AddressList[i].AddressFamily ==
                  AddressFamily.InterNetwork)
                {
                    Client.Ip = ipHostInfo.AddressList[i];
                    break;
                }
            }

            Client.Port = port;
            Client.isConnedted = true;
            label.Content = "Ready !";
            Client.StartListening();
        }
    }
}
