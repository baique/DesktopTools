using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DesktopTools.views
{
    /// <summary>
    /// GoodbyeMode.xaml 的交互逻辑
    /// </summary>
    public partial class GoodbyeMode : Window
    {
        public GoodbyeMode()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            Storyboard Run1 = FindResource("Storyboard1") as Storyboard;
            Run1.Begin();
        }

        private void Completed(object sender, EventArgs e)
        {
            Storyboard Run1 = FindResource("Storyboard1") as Storyboard;
            Run1.Stop();
            this.Close();
        }
    }
}
