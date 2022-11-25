using DesktopTools.component;
using DesktopTools.util;
using DesktopTools.views;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using static DesktopTools.util.Win32;
using Application = System.Windows.Application;
using DateTime = System.DateTime;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxOptions = System.Windows.Forms.MessageBoxOptions;

namespace DesktopTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 托盘菜单
        /// </summary>
        public static NotifyIcon Notify { get; } = Notify = new NotifyIcon
        {
            Text = @"桌面工具",
            Icon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/icon.ico")).Stream),
            Visible = true
        };
        /// <summary>
        /// 设置界面
        /// </summary>
        private Setting? opendSettingView = null;
        /// <summary>
        /// 菜单界面故事板
        /// </summary>
        private Storyboard? prevStoryboard;
        /// <summary>
        /// 退出菜单动画状态
        /// </summary>
        private bool outStatus = false;
        /// <summary>
        /// 禁用录屏
        /// </summary>
        private DisablePrintScreenView? pvView;
        /// <summary>
        /// 菜单自动隐藏
        /// </summary>
        private DispatcherTimer timeoutHide = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.5),
        };


        public MainWindow()
        {
            this.Width = 0;
            this.Height = 0;
            InitializeComponent();
            //初始化托盘区菜单
            InitNotifyIcon();
        }

        /// <summary>
        /// 初始化窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ToggleMainWindow();
            HeartbeatStart(null, null);
            RegisterTimeJump();

            GlobalKeyboardEvent.InitKeyWatch();
            ToggleWindow.Register();
            ToggleWindow.RestoreKeyWindow();
            RegisterAutoChangeBackground();
            RegisterGoodbyeMode();
            RegisterDisableAutoLockScreen();
            RegisterKeyboardEvent();
            //RegisterDisablePrintScreen();

            ToggleWindow.addIgnorePtr(this);
            AppUtil.ExcludeFromCapture(this);
            AppUtil.DisableAltF4(this);
            AppUtil.AlwaysToTop(this);
            AppUtil.HideAltTab(this);
        }

        #region 禁止录屏
        private void RegisterDisablePrintScreen()
        {
            if ("1".Equals(SettingUtil.GetSetting(SettingUtil.DisablePrintScreenKey)))
            {
                if (pvView != null)
                {
                    return;
                }
                pvView = new DisablePrintScreenView();
                pvView.Show();
                pvView.Closed += (a, e) =>
                {
                    pvView = null;
                };
            }
        }
        private void ToggleDisablePrintScreen()
        {
            if (pvView == null)
            {
                pvView = new DisablePrintScreenView();
                pvView.Show();
                pvView.Closed += (a, e) =>
                {
                    pvView = null;
                };
            }
            else
            {

                pvView.Close();
            }
        }
        #endregion

        #region 注册键盘事件
        private void RegisterKeyboardEvent()
        {
            //需要注意此处有一定的顺序要求
            //禁用自动锁屏
            GlobalKeyboardEvent.Register(new DisableAutoLockScreen());
            //录屏
            //GlobalKeyboardEvent.Register(
            //    () => SettingUtil.GetSettingOrDefValueIfNotExists(SettingUtil.ChangeDisablePrintScreenStateKey, "LeftAlt + M"),
            //    (e) =>
            //    {
            //        ToggleDisablePrintScreen();
            //        return true;
            //    }
            //);
            //壁纸切换
            GlobalKeyboardEvent.Register(new SystemBackground());
            //紧急避险
            GlobalKeyboardEvent.Register(
                () => SettingUtil.GetSettingOrDefValueIfNotExists(SettingUtil.ErrorModeKey, "LeftCtrl + LeftShift + Space"),
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
                () => SettingUtil.GetSettingOrDefValueIfNotExists(SettingUtil.UnWindowBindOrChangeKey, "LeftCtrl + LeftAlt + Back"),
                e =>
                {
                    if (MessageBox.Show("当前窗体将被移除全部快捷访问,是否继续？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification) == System.Windows.Forms.DialogResult.Yes) ToggleWindow.RemoveKeyWindow();
                    return true;
                }
            );
            //强制注册快捷键到窗体
            GlobalKeyboardEvent.Register(() => SettingUtil.GetSettingOrDefValueIfNotExists(SettingUtil.ForceWindowBindOrChangeKey, "LeftCtrl + LeftAlt"), e =>
            {
                if ((e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key >= Key.D0 && e.Key <= Key.D9)
                {
                    ToggleWindow.RegisterKeyWindow(e.Key, GetForegroundWindow());
                    return true;
                };
                return false;
            });
            //注册或切换窗体状态
            GlobalKeyboardEvent.Register(() => SettingUtil.GetSettingOrDefValueIfNotExists(SettingUtil.WindowBindOrChangeKey, "LeftCtrl"), e =>
            {
                if ((e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key >= Key.D0 && e.Key <= Key.D9)
                {
                    if (ToggleWindow.ContainsKey(e.Key)) ToggleWindow.ToggleWindowToTop(e.Key);
                    else ToggleWindow.RegisterKeyWindow(e.Key, GetForegroundWindow());
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
                if (!"1".Equals(SettingUtil.GetSetting(SettingUtil.EnableDisableLockScreenKey, "1"))) return;
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
            timer.Tick += (a, e) => { if (GoodbyeModeComponent.IsInGoodbyeTime()) GoodbyeModeComponent.Show(); };
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
        private void RegisterAutoChangeBackground()
        {

            DispatcherTimer autoChangeBackgroundTimer = new DispatcherTimer();
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
            Notify.ContextMenuStrip = new ContextMenuStrip();
            {
                var item = new ToolStripLabel();
                item.Text = "设置";
                item.Click += (o, e) => OpenSettingView(null, null);
                Notify.ContextMenuStrip.Items.Add(item);
            }
            {
                var item = new ToolStripLabel();
                item.Text = "退出";
                item.Click += (o, e) => App.Current.Shutdown();
                Notify.ContextMenuStrip.Items.Add(item);
            }
        }

        /// <summary>
        /// 根据用户设置决定将窗体挪到工作区之外，还是正常显示
        /// </summary>
        private void ToggleMainWindow()
        {
            if ("1".Equals(SettingUtil.GetSetting(SettingUtil.HiddenTimeWindowKey)))
            {
                Dispatcher.Invoke(() =>
                {
                    this.border.Visibility = Visibility.Collapsed;
                    this.Width = 1;
                    this.Height = 1;
                    this.Top = -2;
                    this.Left = -2;
                });
                return;
            }
            else
            {
                MiniWindow();
            }
        }

        /// <summary>
        /// 正常显示窗体
        /// 
        /// 过程中将从注册表读取用户最后一次移动后的窗体位置
        /// </summary>
        private void MiniWindow()
        {
            this.border.Visibility = Visibility.Visible;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            var left = SettingUtil.GetSetting("main-view-left");
            var top = SettingUtil.GetSetting("main-view-top");
            if (string.IsNullOrWhiteSpace(left))
            {
                double x = SystemParameters.WorkArea.Width;
                this.Left = x - this.border.Width;
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
                //Trace.WriteLine("当前高度：" + top + "-" + this.border.Height);
                this.Top = double.Parse(top);
                if (this.Top + this.border.Height < 0) this.Top = -1;
            }
        }

        /// <summary>
        /// 触发窗体移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
                //移动完成更新现在位置
                SettingUtil.SetSetting("main-view-left", "" + this.Left);
                SettingUtil.SetSetting("main-view-top", "" + this.Top);
#if DEBUG
                Trace.WriteLine("更新高度：" + this.Top);
#endif
            }
            catch { }
        }

        /// <summary>
        /// 窗体关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 切换菜单显示状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleMenuVisible(object sender, MouseButtonEventArgs e)
        {
            if (this.MenuView.Visibility == Visibility.Collapsed)
            {
                if (sender == null)
                {
                    return;
                }
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

        /// <summary>
        /// 退出动画结束后修改状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeOutStatus(object sender, EventArgs e)
        {
            outStatus = false;
        }

        /// <summary>
        /// 打开设置界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                if (!"1".Equals(SettingUtil.GetSetting(SettingUtil.EnableViewHeartbeatKey, ""))) HeartbeatStop(null, null);
                else HeartbeatStart(null, null);
                ToggleMainWindow();
                RegisterDisablePrintScreen();
            };
        }

        /// <summary>
        /// 退出应用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitApp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region 呼吸效果
        private void HeartbeatStop(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((Storyboard)this.FindResource("Storyboard1")).Stop();
            this.border.Opacity = 1;
            if (timeoutHide != null) timeoutHide.Stop();
        }

        private void HeartbeatStart(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (opendSettingView != null) return;
            if ("1".Equals(SettingUtil.GetSetting(SettingUtil.EnableViewHeartbeatKey, ""))) ((Storyboard)this.FindResource("Storyboard1")).Begin();
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

        #region 娱乐模式
        /// <summary>
        /// 娱乐模式，跟随鼠标位置运动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WithMouse(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (this.MenuView.Visibility == Visibility.Visible) return;
            if (this.opendSettingView != null) return;
            if (!"1".Equals(SettingUtil.GetSetting(SettingUtil.EnableGameTimeKey))) return;
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
        #endregion
    }
}
