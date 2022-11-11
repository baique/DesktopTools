using DesktopTools.component;
using DesktopTools.views;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using static DesktopTools.util.Win32;
using Application = System.Windows.Application;
using DateTime = System.DateTime;

namespace DesktopTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static NotifyIcon Notify { get; private set; }
        private GlobalKeyboardEvent KeyboardEvent;
        public MainWindow()
        {
            InitializeComponent();
            InitNotifyIcon();
            KeyboardEvent = new GlobalKeyboardEvent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ToggleWindow.addIgnorePtr(this);
            HideAltTab(new WindowInteropHelper(this).Handle);
            MiniWindow();
            RegisterTimeJump();
            RegisterAutoChangeBackground();
            RegisterGoodbyeMode();
            RegisterDisableAutoLockScreen();
            RegisterKeyboardEvent();
        }

        #region 注册键盘事件
        private void RegisterKeyboardEvent()
        {
            //禁用自动锁屏
            GlobalKeyboardEvent.Register(new DisableAutoLockScreen());
            //壁纸切换
            GlobalKeyboardEvent.Register(new SystemBackground());
            //紧急避险
            GlobalKeyboardEvent.Register(
                Setting.GetSetting(Setting.ErrorModeKey, "LeftCtrl + LeftShift + Space"),
                e =>
                {
                    GlobalKeyboardEvent.GlobalKeybordEventStatus = false;
                    WindowUpdate loading = new WindowUpdate();
                    loading.ShowDialog();
                    GlobalKeyboardEvent.GlobalKeybordEventStatus = true;
                }
            );
            //强制注册快捷键到窗体
            GlobalKeyboardEvent.Register(Setting.GetSetting(Setting.ForceWindowBindOrChangeKey, "LeftCtrl + LeftAlt"), e =>
            {
                if ((e.KeyValue >= (int)Keys.NumPad0 && e.KeyValue <= (int)Keys.NumPad9) || e.KeyValue >= (int)Keys.D0 && e.KeyValue <= (int)Keys.D9)
                {
                    ToggleWindow.RegisterKeyWindow(e.KeyData, GetForegroundWindow());
                }
            });
            //注册或切换窗体状态
            GlobalKeyboardEvent.Register(Setting.GetSetting(Setting.WindowBindOrChangeKey, "LeftCtrl"), e =>
            {
                if ((e.KeyValue >= (int)Keys.NumPad0 && e.KeyValue <= (int)Keys.NumPad9) || e.KeyValue >= (int)Keys.D0 && e.KeyValue <= (int)Keys.D9)
                {
                    if (ToggleWindow.ContainsKey(e.KeyData))
                    {
                        ToggleWindow.ToggleWindowToTop(e.KeyData);
                    }
                    else
                    {
                        ToggleWindow.RegisterKeyWindow(e.KeyData, GetForegroundWindow());
                    }
                }
            });

        }
        #endregion

        #region 禁止自动锁屏
        private void RegisterDisableAutoLockScreen()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += (a, e) =>
            {
                if (!"1".Equals(Setting.GetSetting(Setting.EnableDisableLockScreenKey, "1")))
                {
                    return;
                }
                DisableAutoLockScreen.TriggerUserMouseEvent();
            };
            timer.Start();
        }
        #endregion

        #region 挥手模式

        private void RegisterGoodbyeMode()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += (a, e) =>
            {
                if (GoodbyeModeComponent.IsInGoodbyeTime())
                {
                    GoodbyeModeComponent.Show();
                }
            };
            timer.Start();
        }
        #endregion

        #region 时间跳动
        private void RegisterTimeJump()
        {
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(1000);
            timer.Tick += (a, e) =>
            {
                var now = DateTime.Now;
                this.TimeNumber.Text = now.ToString("HH:mm:ss");
                this.DateNumber.Text = now.ToString("MM-dd ddd");
            };
            timer.Start();
        }
        #endregion

        #region 自动切换壁纸
        private DispatcherTimer? autoChangeBackgroundTimer = null;
        private void RegisterAutoChangeBackground()
        {
            autoChangeBackgroundTimer = new DispatcherTimer();
            autoChangeBackgroundTimer.Interval = new TimeSpan(1, 0, 0);
            autoChangeBackgroundTimer.Tick += (a, e) =>
            {
                SystemBackground.ChangeBackground();
            };
            autoChangeBackgroundTimer.Start();

            DispatcherTimer checkTimer = new DispatcherTimer();
            checkTimer.Interval = new TimeSpan(0, 0, 10);
            checkTimer.Tick += (a, e) =>
            {
                if (!"1".Equals(Setting.GetSetting(Setting.EnableBiYingKey)))
                {
                    return;
                }
                SystemBackground.ChangeBackgroundIfModify();
            };
            checkTimer.Start();
        }
        #endregion

        #region 其他
        private void InitNotifyIcon()
        {
            Notify = new NotifyIcon
            {
                Text = @"桌面工具",
                Icon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/icon.ico")).Stream),
                Visible = true
            };
            Notify.ContextMenuStrip = new ContextMenuStrip();
            Notify.BalloonTipTitle = "桌面工具";
            ToolStripItem item = new ToolStripLabel();
            item.Text = "设置";
            item.Click += (o, e) =>
            {
                Setting setting = new Setting();
                setting.Show();
            };
            Notify.ContextMenuStrip.Items.Add(item);
            item = new ToolStripLabel();
            item.Text = "退出";
            item.Click += (o, e) =>
            {
                App.Current.Shutdown();
            };
            Notify.ContextMenuStrip.Items.Add(item);

        }
        private void MiniWindow()
        {
            this.Topmost = true;
            this.SizeToContent = SizeToContent.WidthAndHeight;


            var left = Setting.GetSetting("main-view-left");
            var top = Setting.GetSetting("main-view-top");
            if (string.IsNullOrWhiteSpace(left))
            {
                double x = SystemParameters.WorkArea.Width;
                this.Left = x - this.Width - 20;
            }
            else
            {
                this.Left = double.Parse(left);
            }

            if (string.IsNullOrWhiteSpace(top))
            {

                this.Top = 20;
            }
            else
            {
                this.Top = double.Parse(top);
            }
            //保证当前窗体永远前置
            SetWindowPos(new WindowInteropHelper(this).Handle, -1, 0, 0, 0, 0, 3);
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
                Setting.SetSetting("main-view-left", "" + this.Left);
                Setting.SetSetting("main-view-top", "" + this.Top);
            }
            catch { }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                GlobalKeyboardEvent.close();
                Notify.Visible = false;
                Notify.Dispose();
            }
            catch
            {

            }
            finally
            {
                App.Current.Shutdown();
            }

        }

        private void ToggleKeyMapPanel(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2)
            {
                return;
            }
            ToggleWindow.ToggleIconPanel();
        }
        #endregion
    }
}
