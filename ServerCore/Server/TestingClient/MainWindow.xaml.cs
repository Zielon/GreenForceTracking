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
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            try{
                await Client.Send(this.textBox.Text);
            }
            catch (Exception ex){
                textBoxResponse.Text = ex.Message;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Client.window = this;
            label.Content = "Ready !";
            label.Foreground = new SolidColorBrush(Colors.Green);

            var progress = new Progress<string>(s => this.textBoxResponse.Text = s);

            Task.Factory.StartNew(() => Client.Process(progress), TaskCreationOptions.LongRunning);
        }
    }
}
