using System;
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
            buttonSend.IsEnabled = false;
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
            Client.Login(NameBox.Text, passwordBox.Password).ContinueWith(
            t =>
            {
                if (!t.Result) return;
                label.Content = "Ready !";
                label.Foreground = new SolidColorBrush(Colors.Green);
                buttonSend.IsEnabled = true;
                button.IsEnabled = false;
                Client.StartProcessing();
            },
            TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
