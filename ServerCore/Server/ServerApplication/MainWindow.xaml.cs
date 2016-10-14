using Library.Server;
using Library.API;
using System.Windows;
using System.Windows.Media;


namespace ServerApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static bool Running;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (Running) return;

            Running = true;
            var ip = textBoxIP.Text;

            var server = new Server(ip, 52400);

            dataGrid.DataContext = server.Container.RecivedMessages;

            // Events setup
            server.ContainerEvent += (s, a) => { if (a.Clean) Dispatcher.Invoke(() => server.Container.RecivedMessages.Remove(m => a.Clean)); };
            server.ContainerEvent += (s, a) => { if (a.Message != null) Dispatcher.Invoke(() => server.Container.RecivedMessages.Add(a.Message)); };
            server.MessageEvent += (s, a) => Dispatcher.Invoke(() => textBox.AppendText(a.Message));
            server.WindowEvent += (s, a) => ServerStatus.Content = a.Running;
            server.WindowEvent += (s, a) => { if (a.ChangeBrush) ServerStatus.Foreground = new SolidColorBrush(Colors.Green); };

            server.StartListening();

        }
    }
}
