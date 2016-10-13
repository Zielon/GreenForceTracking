using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;


namespace ServerApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Server server;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

            var ip = textBoxIP.Text;
            if (Server.isRunning) return;
            server = new Server(ip, 52400, this);
            server.StartListening();
        }
    }
}
