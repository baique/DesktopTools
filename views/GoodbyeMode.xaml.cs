using DesktopTools.util;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Drawing;
using System.IO.IsolatedStorage;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using static DesktopTools.util.Win32;

namespace DesktopTools.views
{
    /// <summary>
    /// GoodbyeMode.xaml 的交互逻辑
    /// </summary>
    public partial class GoodbyeMode : Window
    {
        private int? mode = null;
        public GoodbyeMode(int? r = null)
        {
            InitializeComponent();
            this.Topmost = true;
            mode = r;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            AppUtil.ExcludeFromCapture(this);
            AppUtil.HideAltTab(this);
            ((Storyboard)FindResource("Storyboard1")).Begin();
            ((Storyboard)FindResource("Storyboard2")).Begin();
        }


        private void Move(double left, double top, int useTime = 200)
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
                    ef.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(useTime));
                    ef.Value = left;
                    vs.KeyFrames.Add(ef);
                    this.BeginAnimation(Window.LeftProperty, vs);
                }

                {
                    DoubleAnimationUsingKeyFrames vs = new DoubleAnimationUsingKeyFrames();
                    EasingDoubleKeyFrame eft = new EasingDoubleKeyFrame();
                    eft.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0));
                    eft.Value = this.Top;
                    vs.KeyFrames.Add(eft);
                    eft = new EasingDoubleKeyFrame();
                    eft.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(useTime));
                    eft.Value = top;
                    vs.KeyFrames.Add(eft);
                    this.BeginAnimation(Window.TopProperty, vs);
                }
            });
            Thread.Sleep(useTime);
        }

        private void Completed(object sender, EventArgs e)
        {
        }

        private void Go(object sender, EventArgs e)
        {
            int v = (int)(SystemParameters.WorkArea.Width / this.Width);
            int h = (int)(SystemParameters.WorkArea.Height / this.Height);
            double tv = this.Width;
            double th = this.Height;
            int maxTop = (int)(SystemParameters.WorkArea.Height - this.Height);
            int maxLeft = (int)(SystemParameters.WorkArea.Width - this.Width);

            Task.Factory.StartNew(() =>
            {
                switch (mode != null ? mode : new Random().Next(3) % 3)
                {
                    case 0:
                        lineMove(v, h, tv, th, maxLeft, maxTop, false);
                        break;
                    case 1:
                        lineMove(v, h, tv, th, maxLeft, maxTop, true);
                        break;
                    default:
                        randomMove(v, h, maxLeft, maxTop);
                        break;
                }

            });
        }

        private void randomMove(int v, int h, double tv, double th)
        {
            Random r = new Random();
            var f = r.Next(20, 30);
            for (int i = 0; i < f; i++)
            {
                Move(r.Next(0, (int)tv), r.Next(0, (int)th));
            }
            EndAndClose(tv, th);

        }

        private void lineMove(int v, int h, double tv, double th, double maxLeft, double maxTop, bool f)
        {
            if (f)
            {
                v = v ^ h;
                h = v ^ h;
                v = v ^ h;
            };
            bool l = true;
            for (double i = 0; i < v; i++)
            {
                for (double j = 0; j < h; j++)
                {
                    if (f)
                        Move(l ? j * tv : (h - j) * tv, i * th);
                    else
                        Move(i * tv, l ? j * th : (h - j) * th);

                }
                l = !l;
            }
            EndAndClose(maxLeft, maxTop);
        }
        private void EndAndClose(double maxLeft, double maxTop)
        {
            if (mode != null)
            {
                Dispatcher.InvokeAsync(() => this.Close());
            }
            Move(maxLeft / 2, maxTop / 2);
            ((Storyboard)FindResource("Storyboard2")).Stop();

            Dispatcher.InvokeAsync(() =>
            {
                this.textBlock.Foreground = new SolidColorBrush(Colors.Black); this.textBlock.FontSize = 92; this.textBlock.Text = "下班！！";
            });

            Thread.Sleep(2000);
            Dispatcher.InvokeAsync(() => this.Close());
        }

        private void ChangeColorContinue(object sender, EventArgs e)
        {
            ((Storyboard)FindResource("Storyboard2")).Begin();
        }
    }
}
