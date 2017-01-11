﻿using Library.API;
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
        private Library.Server.Server server;
        private Stats statsWindow;

        public MainWindow()
        {
            InitializeComponent();
            statsWindow = new Stats();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (Running) return;

            Running = true;
            var ip = textBoxIP.Text;

            server = new Library.Server.Server(ip, 52400);
            server.WindowEvent += (s, a) => ServerStatus.Content = a.Running;

            dataGrid.DataContext = server.Container.RecivedMessages;

            // Events setup
            server.ContainerEvent += (s, a) => { if (a.Clean) Dispatcher.Invoke(() => server.Container.RecivedMessages.Remove(m => a.Clean)); };
            server.ContainerEvent += (s, a) => { if (a.Message != null) Dispatcher.Invoke(() => server.Container.RecivedMessages.Add(a.Message)); };
            server.MessageEvent += (s, a) => Dispatcher.Invoke(() => textBox.AppendText(a.Message));
            server.WindowEvent += (s, a) => { if (a.ChangeBrush) ServerStatus.Foreground = new SolidColorBrush(Colors.Green); button.IsEnabled = false; };

            server.StartListening();

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.textBox.Clear();
        }

        private void button1_Copy_Click(object sender, RoutedEventArgs e)
        {
            statsWindow.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            statsWindow.Close();
        }
    }
}
