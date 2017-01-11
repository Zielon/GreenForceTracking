using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ServerApplication
{
    /// <summary>
    /// Interaction logic for Stats.xaml
    /// </summary>
    public partial class Stats : Window
    {
        private static string search = "";

        public Stats()
        {
            InitializeComponent();
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var str = Server.Stats.StatsHandler.StatsCollection.Select(e => e.Value.ToString());

                    if (search != string.Empty) {
                        str = Server.Stats.StatsHandler.StatsCollection.Where(e => e.Key.Contains(search)).Select(e => e.Value.ToString());
                    }

                    Dispatcher.Invoke(() => textBox.Text = string.Join("--------------------------------------------\n", str));
                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);
        }

        private void textBox1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            search = this.textBox1.Text;
        }

        private void textBox1_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            search = this.textBox1.Text;
        }
    }
}
