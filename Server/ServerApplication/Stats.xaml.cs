using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Server.Stats;

namespace ServerApplication
{
    /// <summary>
    ///     Interaction logic for Stats.xaml
    /// </summary>
    public partial class Stats : Window
    {
        private static string search = "";

        public Stats()
        {
            InitializeComponent();
            Task.Factory.StartNew(
                () => {
                    while (true)
                    {
                        var str = StatsHandler.StatsCollection.Select(e => e.Value.ToString());

                        if (search != string.Empty)
                            str = StatsHandler.StatsCollection.Where(e => e.Key.Contains(search)).Select(e => e.Value.ToString());

                        Dispatcher.Invoke(() => textBox.Text = string.Join("--------------------------------------------\n", str));
                        Thread.Sleep(100);
                    }
                },
                TaskCreationOptions.LongRunning);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            search = textBox1.Text;
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            search = textBox1.Text;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }
    }
}
