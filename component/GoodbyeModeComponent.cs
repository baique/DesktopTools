using DesktopTools.views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace DesktopTools.component
{
    public class GoodbyeModeComponent
    {
        private static List<Window> activeWindows = new List<Window>();
        public static bool IsInGoodbyeTime()
        {
            if (!GlobalEnable)
            {
                return false;
            }
            if (!"1".Equals(Setting.GetSetting(Setting.EnableGoodbyeModeKey)))
            {
                return false;
            }
            var h = int.Parse(Setting.GetSetting(Setting.EnableGoodbyeHKey));
            var m = int.Parse(Setting.GetSetting(Setting.EnableGoodbyeMKey));
            var type = Setting.GetSetting(Setting.GoodbyeModeTypeKey, "正常");
            if (type == "正常")
            {
#if DEBUG 
                if (DateTime.Now.Hour > h || (DateTime.Now.Hour == h && DateTime.Now.Minute >= m))
#else
                if (DateTime.Now.Hour == h && DateTime.Now.Minute == m)
#endif
                {
                    return true;
                }
            }
            else
            {
                if (DateTime.Now.Hour > h || (DateTime.Now.Hour == h && DateTime.Now.Minute >= m))
                {
                    return true;
                }
            }

            return false;
        }
        private static bool Stop = false;
        private static bool _GlobalEnable = true;

        public static bool GlobalEnable
        {
            get { return _GlobalEnable; }
            set
            {
                _GlobalEnable = value;
                if (!value)
                {
                    foreach (var item in new List<Window>(activeWindows))
                    {
                        try { item.Close(); } catch { }
                    }
                    activeWindows.Clear();
                }
            }
        }

        public static bool IsInFlowMode = false;

        public static void Show(MainWindow self)
        {
            var type = Setting.GetSetting(Setting.GoodbyeModeTypeKey, "正常");
            if (type == "追随")
            {
                if (IsInFlowMode) return;
                IsInFlowMode = true;
                Stopwatch stopwatch = new Stopwatch(); stopwatch.Start();
                var rawLeft = self.Left;
                var rawTop = self.Top;
                self.RandomGoodbye();
                Task.Run(async () =>
                {
                    Storyboard? s = null;
                    for (; ; )
                    {
                        if (stopwatch.ElapsedMilliseconds >= 10 * 1000)
                        {
                            s.Pause();
                            s.Stop();
                            Thread.Sleep(1000);
                            await self.Dispatcher.InvokeAsync(() =>
                            {
                                self.Left = rawLeft;
                                self.Top = rawTop;
                                IsInFlowMode = false;
                            });
                            return;
                        }
                        int at = 250;
                        await Task.Delay(at);
                        self.Dispatcher.Invoke(() =>
                        {
                            if (s != null)
                            {
                                s.Stop();
                            }
                            var currentGraphics = System.Drawing.Graphics.FromHwnd(new WindowInteropHelper(Application.Current.MainWindow).Handle);
                            //计算系统Dpi
                            var DpiX = currentGraphics.DpiX / 96;
                            var DpiY = currentGraphics.DpiY / 96;
                            //计算包括了系统dpi的鼠标坐标
                            double MousePositionX = Control.MousePosition.X / DpiX;
                            double MousePositionY = Control.MousePosition.Y / DpiY;
                            var ax = MousePositionX - self.Width / 2;
                            var ay = MousePositionY - self.Height / 2;
                            var bx = self.Left - self.Width / 2;
                            var by = self.Top - self.Height / 2;
                            var d = Math.Sqrt(Math.Pow(ax - bx, 2) + Math.Pow(ay - by, 2));
                            double t = d / 40 * at;

                            s = new Storyboard();
                            {
                                DoubleAnimationUsingKeyFrames df = new DoubleAnimationUsingKeyFrames();
                                EasingDoubleKeyFrame ef = new EasingDoubleKeyFrame();
                                ef.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(t));
                                ef.Value = ax;
                                df.KeyFrames.Add(ef);
                                Storyboard.SetTarget(df, self.SelfWindow);
                                Storyboard.SetTargetProperty(df, new PropertyPath(Window.LeftProperty));
                                s.Children.Add(df);
                            }
                            {
                                DoubleAnimationUsingKeyFrames df = new DoubleAnimationUsingKeyFrames();
                                EasingDoubleKeyFrame ef = new EasingDoubleKeyFrame();
                                ef.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(t));
                                ef.Value = ay;
                                df.KeyFrames.Add(ef);
                                Storyboard.SetTarget(df, self.SelfWindow);
                                Storyboard.SetTargetProperty(df, new PropertyPath(Window.TopProperty));
                                s.Children.Add(df);
                            }
                            //if (DateTime.Now.Millisecond - lastChangeTime > 1000)
                            //{
                            //    lastChangeTime = DateTime.Now.Millisecond;
                            //    Random rnd = new Random();
                            //    var r = rnd.Next(-1 * randomText.Length * 2, randomText.Length * 2);
                            //    if (r >= 0 && r < randomText.Length - 1)
                            //    {
                            //        self.Tip.Text = randomText[r];
                            //        self.DateNumber.Visibility = Visibility.Collapsed;
                            //        self.Tip.Visibility = Visibility.Visible;
                            //    }
                            //    else
                            //    {
                            //        self.Tip.Visibility = Visibility.Collapsed;
                            //        self.DateNumber.Visibility = Visibility.Visible;
                            //    }
                            //}
                            s.Begin();
                        });
                    }
                });
            }
            else if (type == "遛弯")
            {
                if (Stop)
                {
                    return;
                }
                Stop = true; ;
                GoodbyeMode gm = new GoodbyeMode();
                gm.Show();
                gm.Closed += (a, e) =>
                {
                    activeWindows.Remove(gm);
                    Stop = false;
                };
                activeWindows.Add(gm);
            }
            else if (type == "炸街")
            {
                for (int i = 0; i < 3; i++)
                {
                    var d = new GoodbyeMode(i);
                    d.Closed += (a, e) =>
                    {
                        activeWindows.Remove(d);
                        ;
                    };
                    d.Show();
                    activeWindows.Add(d);
                }
            }
            else
            {
                if (Stop)
                {
                    return;
                }
                Stop = true; ;
                GoodbyeMode2 gm = new GoodbyeMode2();
                gm.Show();
                gm.Closed += (a, e) =>
                {
                    activeWindows.Remove(gm);
                    Stop = false;
                };
                activeWindows.Add(gm);

            }
        }
    }
}
