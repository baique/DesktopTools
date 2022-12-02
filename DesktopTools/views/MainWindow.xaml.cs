using BeanFramework.core;
using DesktopTools.component;
using DesktopTools.util;
using DesktopTools.views;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Application = System.Windows.Application;
using DateTime = System.DateTime;

namespace DesktopTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Context CTX = new Context();
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
            CTX.Start(typeof(MainWindow));
            ToggleMainWindow();
            HeartbeatStart(null, null);
            RegisterTimeJump();
            SettingUtil.SelfPtr.Add(AppUtil.GetHwnd(this));
            AppUtil.ExcludeFromCapture(this);
            AppUtil.DisableAltF4(this);
            AppUtil.AlwaysToTop(this);
            AppUtil.HideAltTab(this);
        }

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
                Notify.Visible = false;
                Notify.Dispose();
            }
            catch { }
            finally
            {
                CTX.Shutdown();
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

            GlobalSystemKeyPressEvent.GlobalKeybordEventStatus = false;
            opendSettingView = new Setting();
            opendSettingView.Show();
            opendSettingView.Closed += (a, e) =>
            {
                ToggleWindow.IconPanel().Refresh();
                opendSettingView = null;
                GlobalSystemKeyPressEvent.GlobalKeybordEventStatus = true;
                if (!"1".Equals(SettingUtil.GetSetting(SettingUtil.EnableViewHeartbeatKey, ""))) HeartbeatStop(null, null);
                else HeartbeatStart(null, null);
                ToggleMainWindow();
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
