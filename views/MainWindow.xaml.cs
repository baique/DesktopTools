using DesktopTools.component;
using DesktopTools.util;
using DesktopTools.views;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using static DesktopTools.util.Win32;
using Application = System.Windows.Application;
using DateTime = System.DateTime;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxOptions = System.Windows.Forms.MessageBoxOptions;
using Point = System.Drawing.Point;

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
            ToggleWindow.Register();
            MiniWindow();
            HeartbeatStart(null, null);
            ToggleWindow.RestoreKeyWindow();
            RegisterTimeJump();
            RegisterAutoChangeBackground();
            RegisterGoodbyeMode();
            RegisterDisableAutoLockScreen();
            RegisterKeyboardEvent();
            AppUtil.DisableAltF4(this);
            AppUtil.AlwaysToTop(this);
            ToggleWindow.addIgnorePtr(this);
            HideAltTab(new WindowInteropHelper(this).Handle);
        }

        #region 注册键盘事件
        private void RegisterKeyboardEvent()
        {
            //需要注意此处有一定的顺序要求
            //禁用自动锁屏
            GlobalKeyboardEvent.Register(new DisableAutoLockScreen());
            //GlobalKeyboardEvent.Register(new DesktopManager());
            //壁纸切换
            GlobalKeyboardEvent.Register(new SystemBackground());
            //紧急避险
            GlobalKeyboardEvent.Register(
                () => Setting.GetSettingOrDefValueIfNotExists(Setting.ErrorModeKey, "LeftCtrl + LeftShift + Space"),
                e =>
                {
                    GlobalKeyboardEvent.GlobalKeybordEventStatus = false;
                    GoodbyeModeComponent.GlobalEnable = false;
                    WindowUpdate loading = new WindowUpdate();
                    loading.ShowDialog();
                    GoodbyeModeComponent.GlobalEnable = true;
                    GlobalKeyboardEvent.GlobalKeybordEventStatus = true;
                    return true;
                }
            );
            //移除快捷键
            GlobalKeyboardEvent.Register(
                () => Setting.GetSettingOrDefValueIfNotExists(Setting.UnWindowBindOrChangeKey, "LeftCtrl + LeftAlt + Back"),
                e =>
                {
                    if (MessageBox.Show("当前窗体将被移除全部快捷访问,是否继续？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification) == System.Windows.Forms.DialogResult.Yes) ToggleWindow.RemoveKeyWindow();
                    return true;
                }
            );
            //强制注册快捷键到窗体
            GlobalKeyboardEvent.Register(() => Setting.GetSettingOrDefValueIfNotExists(Setting.ForceWindowBindOrChangeKey, "LeftCtrl + LeftAlt"), e =>
            {
                if ((e.KeyValue >= (int)Keys.NumPad0 && e.KeyValue <= (int)Keys.NumPad9) || e.KeyValue >= (int)Keys.D0 && e.KeyValue <= (int)Keys.D9)
                {
                    ToggleWindow.RegisterKeyWindow(e.KeyData, GetForegroundWindow());
                    return true;
                };
                return false;
            });
            //注册或切换窗体状态
            GlobalKeyboardEvent.Register(() => Setting.GetSettingOrDefValueIfNotExists(Setting.WindowBindOrChangeKey, "LeftCtrl"), e =>
            {
                if ((e.KeyValue >= (int)Keys.NumPad0 && e.KeyValue <= (int)Keys.NumPad9) || e.KeyValue >= (int)Keys.D0 && e.KeyValue <= (int)Keys.D9)
                {
                    if (ToggleWindow.ContainsKey(e.KeyData)) ToggleWindow.ToggleWindowToTop(e.KeyData);
                    else ToggleWindow.RegisterKeyWindow(e.KeyData, GetForegroundWindow());
                    return true;
                }
                return false;
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
                if (!"1".Equals(Setting.GetSetting(Setting.EnableDisableLockScreenKey, "1"))) return;
                DisableAutoLockScreen.TriggerUserMouseEvent();
            };
            timer.Start();
        }
        #endregion

        #region 挥手模式

        private void RegisterGoodbyeMode()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(45);
            timer.Tick += (a, e) =>
            {
                if (GoodbyeModeComponent.IsInGoodbyeTime()) GoodbyeModeComponent.Show(this);
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
            autoChangeBackgroundTimer.Interval = new TimeSpan(0, 2, 0);
#if DEBUG
            autoChangeBackgroundTimer.Interval = new TimeSpan(0, 0, 10);
#endif
            autoChangeBackgroundTimer.Tick += (a, e) =>
            {
                if (DateTime.Now.Subtract(SystemBackground.getLastChangeBackgroundTime()).TotalMinutes >= 60) SystemBackground.ChangeBackground();
            };
            autoChangeBackgroundTimer.Start();

            DispatcherTimer checkTimer = new DispatcherTimer();
            checkTimer.Interval = new TimeSpan(0, 0, 10);
            checkTimer.Tick += (a, e) => SystemBackground.ChangeBackgroundIfModify();
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
            var item = new ToolStripLabel();
            item.Text = "退出";
            item.Click += (o, e) => App.Current.Shutdown();
            Notify.ContextMenuStrip.Items.Add(item);

        }
        private void MiniWindow()
        {
            this.border.Visibility = Visibility.Visible;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            var left = Setting.GetSetting("main-view-left");
            var top = Setting.GetSetting("main-view-top");
            if (string.IsNullOrWhiteSpace(left))
            {
                double x = SystemParameters.WorkArea.Width;
                this.Left = x - this.Width;
            }
            else
            {
                this.Left = double.Parse(left);
                if (this.Left + this.Width > SystemParameters.WorkArea.Width)
                {
                    double x = SystemParameters.WorkArea.Width;
                    this.Left = x - this.Width;
                }
            }

            if (string.IsNullOrWhiteSpace(top)) this.Top = -10;
            else
            {
                this.Top = double.Parse(top);
                if (this.Top - this.Height < 0) this.Top = -1;
            }
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (GoodbyeModeComponent.IsInFlowMode)
            {
                return;
            }
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
                ToggleWindow.Close();
                GlobalKeyboardEvent.close();
                Notify.Visible = false;
                Notify.Dispose();
            }
            catch { }
            finally
            {
                App.Current.Shutdown();
            }

        }

        private void ToggleKeyMapPanel(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2) return;
            ToggleWindow.ToggleIconPanel();
        }
        #endregion

        #region 呼吸效果
        private DispatcherTimer timeoutHide = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.5),
        };
        private void HeartbeatStop(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((Storyboard)this.FindResource("Storyboard1")).Stop();
            Storyboard sb = new Storyboard();
            this.border.Opacity = 1;
            if (timeoutHide != null) timeoutHide.Stop();
        }

        private void HeartbeatStart(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (opendSettingView != null) return;
            if ("1".Equals(Setting.GetSetting(Setting.EnableViewHeartbeatKey, ""))) ((Storyboard)this.FindResource("Storyboard1")).Begin();
            StopWithMouse();

            if (this.MenuView.Visibility == Visibility.Visible && !outStatus)
            {
                timeoutHide.Tick += (a, e) =>
                {
                    ((Storyboard)this.FindResource("SettingButtonInView")).Stop();
                    ((Storyboard)this.FindResource("SettingButtonOutView")).Begin();
                    timeoutHide.Stop();
                };
                timeoutHide.Start();
            }

        }
        #endregion

        private bool outStatus = false;
        private void ToggleMenuVisible(object sender, MouseButtonEventArgs e)
        {
            if (this.MenuView.Visibility == Visibility.Collapsed)
            {
                ((Storyboard)this.FindResource("SettingButtonOutView")).Stop();
                ((Storyboard)this.FindResource("SettingButtonInView")).Begin();
                if (prevStoryboard != null)
                {
                    prevStoryboard.Stop();
                }
            }
            else
            {
                ((Storyboard)this.FindResource("SettingButtonInView")).Stop();
                ((Storyboard)this.FindResource("SettingButtonOutView")).Begin();
                outStatus = true;
                if (prevStoryboard != null)
                {
                    prevStoryboard.Stop();
                }
            }

        }

        private void ChangeOutStatus(object sender, EventArgs e)
        {
            outStatus = false;
        }

        private Setting opendSettingView = null;
        private void OpenSettingView(object sender, MouseButtonEventArgs e)
        {
            ToggleMenuVisible(sender, e);
            if (opendSettingView != null)
            {
                if (opendSettingView.WindowState == WindowState.Minimized)
                {
                    opendSettingView.WindowState = WindowState.Normal;
                }
                opendSettingView.Focus();
                return;
            }

            GlobalKeyboardEvent.GlobalKeybordEventStatus = false;
            opendSettingView = new Setting();
            opendSettingView.Show();
            opendSettingView.Closed += (a, e) =>
            {
                ToggleWindow.IconPanel().Refresh();
                opendSettingView = null;
                GlobalKeyboardEvent.GlobalKeybordEventStatus = true;
                if (!"1".Equals(Setting.GetSetting(Setting.EnableViewHeartbeatKey, ""))) HeartbeatStop(null, null);
                else HeartbeatStart(null, null);
            };
        }


        private static string[] randomText = new string[]
        {
            "下班了喂！",
            "溜了溜了！",
            "你还卷！",
            "别卷了！",
            "走了~卷王",
        };

        public void RandomGoodbye()
        {
            this.DateNumber.Visibility = Visibility.Collapsed;
            this.Tip.Visibility = Visibility.Visible;
            Dispatcher.Invoke(() =>
            {
                Random rnd = new Random();
                var r = rnd.Next(0, randomText.Length);
                this.Tip.Text = randomText[r];
            });
        }

        private void ExitApp(object sender, MouseButtonEventArgs e)
        {
            if (GoodbyeModeComponent.IsInFlowMode)
            {
                this.DateNumber.Visibility = Visibility.Collapsed;
                this.Tip.Visibility = Visibility.Visible;
                Task.Run(() =>
                {
                    Dispatcher.Invoke(() => this.Tip.Text = "休想！");
                    Thread.Sleep(2000);
                });
                return;
            }
            this.Close();
        }

        private Storyboard prevStoryboard;
        private void WithMouse(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (this.MenuView.Visibility == Visibility.Visible) return;
            if (this.opendSettingView != null) return;
            if (!"1".Equals(Setting.GetSetting(Setting.EnableGameTimeKey))) return;
            if (prevStoryboard != null) prevStoryboard.Stop();

            FrameworkElement item = this.border;
            var point = e.GetPosition(item);
            double hc = item.Height / 2, wc = item.Width / 2;
            double a = 0, x = wc, y = hc;
            int leftOrRight = 0;
            if (point.Y < hc && point.X > wc)
            {
                leftOrRight = -1;
                y = hc - point.Y;
                x = point.X - wc;
            }
            else if (point.Y < hc && point.X < wc)
            {
                leftOrRight = 1;
                y = hc - point.Y;
                x = wc - point.X;
            }
            else if (point.Y > hc && point.X > wc)
            {
                leftOrRight = 1;
                y = point.Y - hc;
                x = point.X - wc;
            }
            else if (point.Y > hc && point.X < wc)
            {
                leftOrRight = -1;
                y = point.Y - hc;
                x = wc - point.X;
            }

            a = (Math.Atan2(y, x) * 180 / Math.PI) % 18;

            if (!GoodbyeModeComponent.IsInFlowMode)
            {
                this.Tip.FontSize = 16;
                if (a <= 0.0000001)
                {
                    this.Tip.Text = "(" + a.ToString("0.00000000") + ")神乎其技";
                    this.Tip.FontSize = 12;
                }
                else if (a <= 0.000001)
                {
                    this.Tip.Text = "(" + a.ToString("0.0000000") + ")完美平衡";
                    this.Tip.FontSize = 12.5;
                }
                else if (a <= 0.2)
                {
                    this.Tip.Text = "(" + a.ToString("0.00") + ")算你达成";
                }
                else if (a <= 1)
                {
                    this.Tip.Text = "(" + a.ToString("0.0") + ")接近平衡";
                }
                else if (a <= 2)
                {
                    this.Tip.Text = "(" + a.ToString("0") + ")就差一点";
                }
                else if (a <= 4)
                {
                    this.Tip.Text = "(" + a.ToString("0") + ")就差亿点";
                }
                else
                {
                    this.Tip.Text = "(" + a.ToString("0") + ")歪的厉害";
                }
                this.DateNumber.Visibility = Visibility.Collapsed;
                this.Tip.Visibility = Visibility.Visible;
            }
            a = a * leftOrRight;

            Storyboard sb = new Storyboard();
            DoubleAnimationUsingKeyFrames doubleAnimation = new DoubleAnimationUsingKeyFrames();
            {
                EasingDoubleKeyFrame ek = new EasingDoubleKeyFrame();
                ek.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.2));
                ek.Value = a;
                doubleAnimation.KeyFrames.Add(ek);
            }
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[2].(RotateTransform.Angle)"));
            Storyboard.SetTarget(doubleAnimation, item);


            sb.Children.Add(doubleAnimation);
            sb.Begin();
            prevStoryboard = sb;

        }
        private void StopWithMouse()
        {
            var s = new Storyboard();
            {
                DoubleAnimationUsingKeyFrames doubleAnimation = new DoubleAnimationUsingKeyFrames();
                {
                    EasingDoubleKeyFrame ek = new EasingDoubleKeyFrame();
                    ek.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.2));
                    ek.Value = 0;
                    doubleAnimation.KeyFrames.Add(ek);
                }
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[2].(RotateTransform.Angle)"));
                Storyboard.SetTarget(doubleAnimation, this.border);
                s.Children.Add(doubleAnimation);
            }
            s.Completed += (a, e) =>
            {
                this.Tip.Visibility = Visibility.Collapsed;
                this.DateNumber.Visibility = Visibility.Visible;
            };
            s.Begin();
        }
    }
}
