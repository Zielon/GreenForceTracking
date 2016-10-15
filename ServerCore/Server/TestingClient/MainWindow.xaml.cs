using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;



namespace TestingClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Client Client;

        public MainWindow()
        {
            InitializeComponent();
            Client = new Client(new Progress<string>(s => this.textBoxResponse.Text = s), this);
        }

        private async void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Client.Send(this.textBox.Text);
            }
            catch (Exception ex)
            {
                textBoxResponse.Text = ex.Message;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            label.Content = "Ready !";
            label.Foreground = new SolidColorBrush(Colors.Green);
            Client.StartProcessing();
        }
    }
}
