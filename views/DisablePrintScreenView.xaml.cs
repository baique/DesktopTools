using DesktopTools.component;
using DesktopTools.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DesktopTools.views
{
    /// <summary>
    /// DisablePrintScreenView.xaml 的交互逻辑
    /// </summary>
    public partial class DisablePrintScreenView : Window
    {
        public DisablePrintScreenView()
        {
            InitializeComponent();
        }

        private void WinLoaded(object sender, RoutedEventArgs e)
        {
            this.Width = SystemParameters.WorkArea.Width + 220;
            
            this.Height = SystemParameters.WorkArea.Height + (SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height - 5);
            ToggleWindow.addIgnorePtr(this);
            AppUtil.MonitorFromCapture(this);
            AppUtil.AlwaysToTop(this);
            AppUtil.HideAltTab(this);
            AppUtil.DisableAltF4(this);
            MainWindow.Notify.ShowBalloonTip(100, "提示", "已进入禁止录屏状态", ToolTipIcon.Info);
        }

        private void BeforeClose(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow.Notify.ShowBalloonTip(50, "提示", "离开禁止录屏状态", ToolTipIcon.Info);
        }
    }
}
