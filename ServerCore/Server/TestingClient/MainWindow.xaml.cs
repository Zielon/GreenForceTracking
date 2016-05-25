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
using ServerApplication;

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
        private async void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            await Client.Send(this.textBox.Text);
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            var dns = textBoxIP.Text;

            //IPHostEntry ipHostInfo = Dns.GetHostEntry(dns);
            //for (int i = 0; i < ipHostInfo.AddressList.Length; ++i)
            //{
            //    if (ipHostInfo.AddressList[i].AddressFamily ==
            //      AddressFamily.InterNetwork)
            //    {
            //        Client.Ip = ipHostInfo.AddressList[i];
            //        break;
            //    }
            //}

            Client.Ip = IPAddress.Parse("192.168.0.2");

            Client.Port = Consts.RecivingPort;
            Client.isConnedted = true;
            label.Content = "Ready !";
            label.Foreground = new SolidColorBrush(Colors.Green);
            Client.StartListening();
        }
    }
}
