using DesktopTools.util;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Drawing;
using System.IO.IsolatedStorage;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static DesktopTools.util.Win32;

namespace DesktopTools.views
{
    /// <summary>
    /// GoodbyeMode.xaml 的交互逻辑
    /// </summary>
    public partial class GoodbyeMode : Window
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool UpdateWindow(IntPtr hWnd);
        public GoodbyeMode()
        {
            InitializeComponent();
            this.Topmost = true;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
            Storyboard Run1 = FindResource("Storyboard1") as Storyboard;
#pragma warning disable CS8602 // 解引用可能出现空引用。
            Run1.Begin();

            HideAltTab(new WindowInteropHelper(this).Handle);
        }

        private void Completed(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
