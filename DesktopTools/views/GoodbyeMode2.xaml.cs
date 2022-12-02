using DesktopTools.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DesktopTools.views
{
    /// <summary>
    /// GoodbyeMode2.xaml 的交互逻辑
    /// </summary>
    public partial class GoodbyeMode2 : Window
    {
        public GoodbyeMode2()
        {
            InitializeComponent();
            this.panel.Visibility = Visibility.Hidden;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if ("1".Equals(SettingUtil.GetSetting(SettingUtil.RandomGoodbyeModeThemeKey)))
            {
                ResourceDictionary resource = new ResourceDictionary();
                resource.Source = new Uri("pack://application:,,,/resource/ColorTheme" + new Random().Next(6) + ".xaml");
                this.Resources.MergedDictionaries.Add(resource);
            }
            this.Width = SystemParameters.WorkArea.Width;
            this.Height = SystemParameters.WorkArea.Height;
            this.WindowState = WindowState.Maximized;
            this.p2.Height = this.p1.Height = this.Height;
            this.p2.Width = this.p1.Width = this.Width / 2;
            this.c1.RenderTransform = new TranslateTransform();
            this.c2.RenderTransform = new TranslateTransform();
            this.p1.RenderTransform = new TranslateTransform();
            this.p2.RenderTransform = new TranslateTransform();
            Canvas.SetTop(this.c1, this.Height / 2 - 220 - 60);
            Canvas.SetTop(this.c2, this.Height / 2 + 60);
            int at = 2;
            Storyboard storyboard = new Storyboard();

            {
                DoubleAnimation d1 = new DoubleAnimation(
                    -110,
                    this.Width / 2 - 110,
                    new Duration(TimeSpan.FromSeconds(at))
                );
                Storyboard.SetTarget(d1, this.c1);
                Storyboard.SetTargetProperty(d1, new PropertyPath("RenderTransform.X"));

                storyboard.Children.Add(d1);
            }
            {
                DoubleAnimation d1 = new DoubleAnimation(
                    this.Width - 110,
                    this.Width / 2 - 110,
                    new Duration(TimeSpan.FromSeconds(at))
                );
                Storyboard.SetTarget(d1, this.c2);
                Storyboard.SetTargetProperty(d1, new PropertyPath("RenderTransform.X"));

                storyboard.Children.Add(d1);
            }
            {
                DoubleAnimation d1 = new DoubleAnimation(
                    this.Width / -2,
                    0,
                    new Duration(TimeSpan.FromSeconds(at))
                );
                Storyboard.SetTarget(d1, this.p1);
                Storyboard.SetTargetProperty(d1, new PropertyPath("RenderTransform.X"));

                storyboard.Children.Add(d1);
            }


            {
                DoubleAnimation d1 = new DoubleAnimation(
                    this.Width / 2,
                    0,
                    new Duration(TimeSpan.FromSeconds(at))
                );
                Storyboard.SetTarget(d1, this.p2);
                Storyboard.SetTargetProperty(d1, new PropertyPath("RenderTransform.X"));

                storyboard.Children.Add(d1);
            }

            Timeline.SetDesiredFrameRate(storyboard, 30);
            storyboard.Begin();
            this.panel.Visibility = Visibility.Visible;
        }
        private bool closeing = false;

        private void CloseThis(object sender, MouseButtonEventArgs e)
        {
            if (closeing)
            {
                return;
            }
            closeing = true;
            int at = 2;
            Storyboard storyboard = new Storyboard();

            {
                DoubleAnimation d1 = new DoubleAnimation(
                    this.Width / 2 - 110,
                    -110,
                    new Duration(TimeSpan.FromSeconds(at))
                );
                Storyboard.SetTarget(d1, this.c1);
                Storyboard.SetTargetProperty(d1, new PropertyPath("RenderTransform.X"));

                storyboard.Children.Add(d1);
            }
            {
                DoubleAnimation d1 = new DoubleAnimation(
                    this.Width / 2 - 110,
                    this.Width - 110,
                    new Duration(TimeSpan.FromSeconds(at))
                );
                Storyboard.SetTarget(d1, this.c2);
                Storyboard.SetTargetProperty(d1, new PropertyPath("RenderTransform.X"));

                storyboard.Children.Add(d1);
            }

            {
                DoubleAnimation d1 = new DoubleAnimation(
                    0,
                    this.Width / -2,
                    new Duration(TimeSpan.FromSeconds(at))
                );
                Storyboard.SetTarget(d1, this.p1);
                Storyboard.SetTargetProperty(d1, new PropertyPath("RenderTransform.X"));
                storyboard.Children.Add(d1);
            }

            {
                DoubleAnimation d1 = new DoubleAnimation(
                    0,
                    this.Width / 2,
                    new Duration(TimeSpan.FromSeconds(at))
                );
                Storyboard.SetTarget(d1, this.p2);
                Storyboard.SetTargetProperty(d1, new PropertyPath("RenderTransform.X"));
                storyboard.Children.Add(d1);
            }

            Timeline.SetDesiredFrameRate(storyboard, 30);
            storyboard.Completed += (a, e) =>
            {
                this.Close();
            };
            storyboard.Begin();

        }
    }
}
