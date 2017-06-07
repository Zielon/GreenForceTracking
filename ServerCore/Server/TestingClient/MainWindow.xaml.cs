using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TestingClient
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Client Client;

        public MainWindow()
        {
            InitializeComponent();
            buttonSend.IsEnabled = false;
            markerAdd.IsEnabled = false;
            Client = new Client(new Progress<string>(s => textBoxResponse.Text += s), this);
        }

        private async void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            try { await Client.Send(textBox.Text); }
            catch (Exception ex) { textBoxResponse.Text = ex.Message; }
        }

        private async void markerAdd_Click(object sender, RoutedEventArgs e)
        {
            try { await Client.SendMarker(); }
            catch (Exception ex) { textBoxResponse.Text = ex.Message; }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Client.Conntect();
            Client.Login(NameBox.Text, passwordBox.Password)
                  .ContinueWith(
                      t => {
                          if (!t.Result)
                          {
                              textBoxResponse.Text = "Wrong password or login !\n";
                              return;
                          }
                          textBoxResponse.Text = "Correct password and login !\n";
                          label.Content = "Ready !";
                          label.Foreground = new SolidColorBrush(Colors.Green);
                          buttonSend.IsEnabled = true;
                          markerAdd.IsEnabled = true;
                          button.IsEnabled = false;
                          Client.StartProcessing();
                      },
                      TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
