using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Color = System.Windows.Media.Color;

namespace DesktopTools
{
    /// <summary>
    /// Loading.xaml 的交互逻辑
    /// </summary>
    public partial class WindowUpdate : Window
    {
        private DispatcherTimer d;
        private int i = 0;

        public WindowUpdate()
        {
            InitializeComponent();
            this.Topmost = true;
            d = new DispatcherTimer();
            d.Interval = new TimeSpan(0, 0, 60);
            d.Tick += D_Tick;
            d.Start();
            D_Tick(null, null);
            i = new Random().Next(10);

        }

        private async void D_Tick(object? sender, EventArgs e)
        {
            var nb = (i += new Random().Next(2));
            if (nb > 100)
            {
                nb = 100;
                await Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        this.pn.Visibility = Visibility.Collapsed;
                        this.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    });
                    Thread.Sleep(3000);
                    Dispatcher.Invoke(() => this.Close());
                });
            }
            this.bfb.Content = nb + "%";
        }

        private void WindowClose(object sender, EventArgs e)
        {
            d.Stop();
        }
    }
}
