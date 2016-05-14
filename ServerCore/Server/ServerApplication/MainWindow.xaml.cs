using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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


namespace ServerApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Server server;
        public static int dsfsdf;
        private TextWriter _writer = null;

        public MainWindow()
        {
            InitializeComponent();
            _writer = new TextBoxStreamWriter(textBox);
            Console.SetOut(_writer);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var ip = textBoxIP.Text;
            if (Server.isRunning) return;
            server = new Server(ip, Consts.RecivingPort, this);
            server.StartListening();
        }
    }

    class TextBoxStreamWriter : TextWriter
    {
        TextBox _output = null;

        public TextBoxStreamWriter(TextBox output)
        {
            _output = output;
        }

        public override void Write(char value)
        {
            base.Write(value);
            _output.AppendText(value.ToString());
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}
