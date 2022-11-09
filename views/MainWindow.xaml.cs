using DesktopTools.model;
using DesktopTools.views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using static DesktopTools.util.Win32;
using Application = System.Windows.Application;
using DateTime = System.DateTime;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using KeyEventHandler = System.Windows.Forms.KeyEventHandler;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxOptions = System.Windows.Forms.MessageBoxOptions;
using Task = System.Threading.Tasks.Task;

namespace DesktopTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static NotifyIcon Notify;
        public static MainWindow Instance;
        public static Dictionary<Keys, WindowInfo> windowBinding = new Dictionary<Keys, WindowInfo>();

        private static KeyboardHook k_hook = new KeyboardHook();
        private bool inTheErrorMode = false;

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MiniWindow();
            RegisterTimeJump();
            RegisterAutoChangeBackground();
            InitNotifyIcon();
            InitKeyWatch();
            RegisterGoodbyeMode();
            RegisterDisableAutoLockScreen();
        }

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
                mouse_event(MouseEventFlag.Move, 0, 0, 0, UIntPtr.Zero);
            };
            timer.Start();
        }



        #region 挥手模式
        private void RegisterGoodbyeMode()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(50);
            timer.Tick += (a, e) =>
            {
                if (!"1".Equals(Setting.GetSetting(Setting.EnableGoodbyeModeKey)))
                {
                    return;
                }
                var h = int.Parse(Setting.GetSetting(Setting.EnableGoodbyeHKey));
                var m = int.Parse(Setting.GetSetting(Setting.EnableGoodbyeMKey));

                if (DateTime.Now.Hour > h + 1 || DateTime.Now.Hour >= h && DateTime.Now.Minute > m)
                {
                    MoveRandom();
                }
            };
            timer.Start();
        }
        private bool ReplayStoped = true;
        private void Replay(object sender, EventArgs e)
        {
            if (ReplayStoped)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    Storyboard f;
                    f = (Storyboard)this.FindResource("Storyboard2");
                    f.Stop();

                    f = (Storyboard)this.FindResource("Storyboard1");
                    f.Stop();
                    this.DateNumber.Visibility = Visibility.Visible;
                    Setting.SetSetting("main-view-left", "" + this.Left);
                    Setting.SetSetting("main-view-top", "" + this.Top);
                });
                return;
            }
            ReplayStoped = false;
            var f = (Storyboard)this.FindResource("Storyboard1");
            f.Begin();
        }
        private async void MoveRandom()
        {
            var i = 0;
            var rLeft = this.Left;
            var rTop = this.Top;
            var left = SystemParameters.WorkArea.Width / 2 - this.Width / 2;
            var top = SystemParameters.WorkArea.Height / 2;

            await Task.Run(() =>
            {
                Move(left, top);
                Dispatcher.InvokeAsync(() =>
                {
                    ReplayStoped = false;
                    Storyboard f;
                    f = (Storyboard)this.FindResource("Storyboard2");
                    f.Begin();

                    f = (Storyboard)this.FindResource("Storyboard1");
                    f.Begin();


                });
                Thread.Sleep(2000);
                do
                {
                    Move(left, top);
                    if ("1".Equals(Setting.GetSetting(Setting.EnableMouseGoodbyeModeKey)))
                    {
                        SetCursorPos((int)left, (int)top);
                    }
                    Dispatcher.InvokeAsync(() =>
                    {
                        string d = ((SystemParameters.WorkArea.Width - this.Width) + "");
                        var l = new Random().NextInt64(0, (long)double.Parse(d));
                        var t = new Random().NextInt64(0, (long)double.Parse((SystemParameters.WorkArea.Height - this.Height) + ""));
                        left = l;
                        top = t;
                    });

                } while (i++ < 60);
                ReplayStoped = true;
                Move(rLeft, rTop);
            });
        }
        private void Move(double left, double top)
        {
            Dispatcher.InvokeAsync(() =>
            {
                {
                    DoubleAnimationUsingKeyFrames vs = new DoubleAnimationUsingKeyFrames();
                    EasingDoubleKeyFrame ef = new EasingDoubleKeyFrame();
                    ef.KeyTime = KeyTime.FromTimeSpan(new TimeSpan(0));
                    ef.Value = this.Left;
                    vs.KeyFrames.Add(ef);
                    ef = new EasingDoubleKeyFrame();
                    ef.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500));
                    ef.Value = left;
                    vs.KeyFrames.Add(ef);
                    this.SelfWindow.BeginAnimation(Window.LeftProperty, vs);
                }

                {
                    DoubleAnimationUsingKeyFrames vs = new DoubleAnimationUsingKeyFrames();
                    EasingDoubleKeyFrame eft = new EasingDoubleKeyFrame();
                    eft.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0));
                    eft.Value = this.Top;
                    vs.KeyFrames.Add(eft);
                    eft = new EasingDoubleKeyFrame();
                    eft.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500));
                    eft.Value = top;
                    vs.KeyFrames.Add(eft);
                    this.SelfWindow.BeginAnimation(Window.TopProperty, vs);
                }
            });
            Thread.Sleep(500);
        }
        #endregion

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
                inTheErrorMode = true;
                try
                {
                    Setting setting = new Setting();
                    setting.ShowDialog();
                }
                finally
                {
                    inTheErrorMode = false;
                }
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
            SetWindowPos(new WindowInteropHelper(bv).Handle, -1, 0, 0, 0, 0, 3);
        }

        #region 全局键盘事件
        private void InitKeyWatch()
        {
            k_hook.KeyDownEvent += new KeyEventHandler(hook_KeyDown);//钩住键按下
            k_hook.Start();//安装键盘钩子
        }
        private void hook_KeyDown(object sender, KeyEventArgs e)
        {
            if (inTheErrorMode)
            {
                return;
            }
            if (CheckKeyDown(Setting.GetSetting(Setting.ErrorModeKey, "LeftCtrl + LeftShift + Space"), e))
            {

                if (!inTheErrorMode)
                {
                    inTheErrorMode = true;
                    Loading loading = new Loading();
                    loading.ShowDialog();
                    inTheErrorMode = false;
                }
            }
            else if (CheckKeyDown(Setting.GetSetting(Setting.ChangeEnableDisableLockScreenKey, "LeftCtrl + LeftAlt + Space"), e))
            {
                if ("1".Equals(Setting.GetSetting(Setting.EnableDisableLockScreenKey, "1")))
                {
                    Setting.SetSetting(Setting.EnableDisableLockScreenKey, "0");
                    Notify.ShowBalloonTip(300, "提示", "禁止自动锁屏已关闭", ToolTipIcon.Info);
                }
                else
                {
                    Setting.SetSetting(Setting.EnableDisableLockScreenKey, "1");
                    Notify.ShowBalloonTip(300, "提示", "禁止自动锁屏已开启", ToolTipIcon.Info);
                }
            }
            else if (CheckKeyDown(Setting.GetSetting(Setting.ChangeBiYingBackgroundKey, "LeftCtrl + LeftAlt + B + N"), e))
            {
                doChangeBackground();
            }
            else if (CheckKeyDown(Setting.GetSetting(Setting.UnWindowBindOrChangeKey, "LeftCtrl + LeftAlt + Back"), e))
            {
                if (MessageBox.Show("当前窗体将被移除全部快捷访问,是否继续？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification) == System.Windows.Forms.DialogResult.Yes)
                {
                    RemoveKeyWindow();
                }
            }
            else if (CheckKeyDown(Setting.GetSetting(Setting.ForceWindowBindOrChangeKey, "LeftCtrl + LeftAlt"), e))
            {
                if ((e.KeyValue >= (int)Keys.NumPad0 && e.KeyValue <= (int)Keys.NumPad9) || e.KeyValue >= (int)Keys.D0 && e.KeyValue <= (int)Keys.D9)
                {
                    var wd = GetForegroundWindow();
                    RegisterKeyWindow(e.KeyData, wd);
                }
            }
            else if (CheckKeyDown(Setting.GetSetting(Setting.WindowBindOrChangeKey, "LeftCtrl"), e))
            {
                if ((e.KeyValue >= (int)Keys.NumPad0 && e.KeyValue <= (int)Keys.NumPad9) || e.KeyValue >= (int)Keys.D0 && e.KeyValue <= (int)Keys.D9)
                {
                    if (windowBinding.ContainsKey(e.KeyData))
                    {
                        ToggleWindowToTop(e.KeyData, windowBinding[e.KeyData]);
                    }
                    else
                    {
                        RegisterKeyWindow(e.KeyData, GetForegroundWindow());
                    }
                }
            }
        }


        private bool CheckKeyDown(string v, KeyEventArgs eventArgs)
        {
            var keys = v.Split(" + ");
            foreach (var k in keys)
            {
                Key key = (Key)Enum.Parse(typeof(Key), k);
                if (key == Key.LeftCtrl || key == Key.RightCtrl)
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        continue;
                    }
                    return false;
                }
                if (key == Key.LeftShift || key == Key.RightShift)
                {
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        continue;
                    }
                    return false;
                }
                if (key == Key.LeftAlt || key == Key.RightAlt)
                {
                    if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                    {
                        continue;
                    }
                    return false;
                }
                if (eventArgs.KeyCode.ToString().Equals(key.ToString()) || Keyboard.IsKeyDown(key))
                {
                    continue;
                }
                return false;
            }
            return true;
        }

        public void ToggleWindowToTop(Keys keyData, WindowInfo info)
        {
            //if (info.P.HasExited)
            //{
            //    if(DontContinueBindOther(keyData))
            //    {
            //        return;
            //    }
            //}
            var ptr = info.Ptr;
            var wd = GetForegroundWindow();
            if (IsIconic(ptr) || !IsWindowVisible(ptr))
            {
                ShowWindow(ptr, 9);
                ActiveWindow(keyData, info);

            }
            else if (wd != ptr)
            {
                ActiveWindow(keyData, info);
            }
            else
            {
                ShowWindow(ptr, 2);
            }
        }

        private bool DontContinueBindOther(Keys keyData)
        {
            RemoveKeyWindow(keyData);
            if (MessageBox.Show(
                    "绑定的进程已结束，是否更换到当前前置窗口？",
                    "提示",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.ServiceNotification
                    ) != System.Windows.Forms.DialogResult.Yes)
            {
                return true;
            }
            return false;
        }

        private void ActiveWindow(Keys keyData, WindowInfo needToTopWindow)
        {
            var fs = GetForegroundWindow();
            var targetInputProcessId = GetWindowThreadProcessId(fs, IntPtr.Zero);
            var curThread = GetCurrentThreadId();
            if (fs == needToTopWindow.Ptr)
            {
                return;
            }

            bool attach = AttachThreadInput(curThread, targetInputProcessId, true);
            try
            {
                SetWindowPos(needToTopWindow.Ptr, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
                SetWindowPos(needToTopWindow.Ptr, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
                if (!SetForegroundWindow(needToTopWindow.Ptr))
                {
                    if (DontContinueBindOther(keyData))
                    {
                        return;
                    }
                    RegisterKeyWindow(keyData, GetForegroundWindow());
                }
            }
            finally
            {
                if (attach)
                {
                    AttachThreadInput(curThread, targetInputProcessId, false);
                }
            }
        }

        private void RegisterKeyWindow(Keys keyData, IntPtr wd)
        {
            //窗体已经被关闭了可能，此处最好执行重新绑定窗体
            if (wd == new WindowInteropHelper(this).Handle || wd == new WindowInteropHelper(bv).Handle)
            {
                return;
            }

            int pid = 0;
            GetWindowThreadProcessId(wd, out pid);
            Process proc = Process.GetProcessById(pid);
            var wi = new WindowInfo
            {
                Title = GetText(wd),
                Ptr = wd,
                P = proc,
                StartFile = proc.MainModule.FileName
            };
            if (String.IsNullOrWhiteSpace(wi.Title))
            {
                if (proc != null)
                {
                    if (proc.ProcessName != null)
                    {
                        wi.Title = proc.ProcessName;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(wi.Title))
            {
                return;
            }
            if (windowBinding.ContainsValue(wi))
            {
                if (MessageBox.Show(
                    "[" + wi.Title + "]已经绑定了一个快捷键，是否仍要绑定？",
                    "提示",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.ServiceNotification
                    ) != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }
            }
            windowBinding[keyData] = wi;
            //Notify.ShowBalloonTip(300, "绑定成功", "[" + wi.Title + "]已绑定到快捷键[" + keyData.ToString() + "]", ToolTipIcon.Info);
            bv.Refresh();
        }

        public void RemoveKeyWindow()
        {
            var w = GetForegroundWindow();
            foreach (var item in windowBinding)
            {
                if (item.Value.Ptr == w)
                {
                    windowBinding.Remove(item.Key);
                }
            }
            bv.Refresh();
        }

        public void RemoveKeyWindow(Keys key)
        {
            windowBinding.Remove(key);
            bv.Refresh();
        }


        public static string GetText(IntPtr hWnd)
        {
            // Allocate correct string length first
            int length = GetWindowTextLength(hWnd);
            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        #endregion

        #region 时间跳动
        private void RegisterTimeJump()
        {
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(1000);
            timer.Tick += TimeChange;
            timer.Start();
        }
        private void TimeChange(object? sender, EventArgs e)
        {
            this.TimeNumber.Text = DateTime.Now.ToString("HH:mm:ss");
            this.DateNumber.Text = DateTime.Now.ToString("MM-dd ddd");
        }
        #endregion

        #region 自动切换壁纸
        DispatcherTimer autoChangeBackgroundTimer = null;
        private void RegisterAutoChangeBackground()
        {
            autoChangeBackgroundTimer = new DispatcherTimer();
            autoChangeBackgroundTimer.Interval = new TimeSpan(1, 0, 0);
            autoChangeBackgroundTimer.Tick += (a, e) =>
            {
                if (!"1".Equals(Setting.GetSetting(Setting.EnableBiYingKey)))
                {
                    return;
                }
                doChangeBackground();
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
                StringBuilder sb = new StringBuilder();
                SystemParametersInfo(0x0073, 65535, sb, 0);
                if (Path.Equals(sb.ToString(), Path.GetTempPath() + "tmp_background.bmp"))
                {
                    return;
                }
                doChangeBackground();
            };
            checkTimer.Start();
        }
        private async void doChangeBackground()
        {
            await Task.Run(() =>
            {
                using (var client = new WebClient())
                {

                    var imgData = client.DownloadData("https://bingw.jasonzeng.dev/?index=random");
                    var image = byteArrayToImage(imgData);
                    Bitmap img = new Bitmap(image);
                    FileInfo f = new FileInfo(Path.GetTempPath() + "/tmp_background.bmp");
                    Dispatcher.InvokeAsync(() =>
                    {
                        img.Save(f.FullName);
                        SystemParametersInfo(0x0014, 0, f.FullName, 2);
                    });

                }
            });
        }
        /// <summary>
        /// byte[]转换成Image
        /// </summary>
        /// <param name="byteArrayIn">二进制图片流</param>
        /// <returns>Image</returns>
        public static System.Drawing.Image byteArrayToImage(byte[] byteArrayIn)
        {
            if (byteArrayIn == null)
                return null;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(byteArrayIn))
            {
                System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms);
                ms.Flush();
                return returnImage;
            }
        }
        #endregion

        #region 其他
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
            k_hook.Stop();
            try
            {
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

        private static BindingView bv = new BindingView();
        private void ToggleKeyMapPanel(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2)
            {
                return;
            }
            if (windowBinding.Count == 0)
            {
                bv.Hide();
                return;
            }
            if (bv.IsVisible)
            {
                bv.Hide();
            }
            else
            {
                bv.Show();
            }
        }
        #endregion

    }
}
