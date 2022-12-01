using DesktopTools.component.impl;
using DesktopTools.component.model;
using DesktopTools.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;

namespace DesktopTools
{
    /// <summary>
    /// Interaction logic for BindingView.xaml
    /// </summary>
    public partial class BindingView : Window
    {
        private string currentFlowMode = "";
        private Debounce debounce;
        private string prevFlowMode = "";
        public BindingView()
        {
            InitializeComponent();
            debounce = new Debounce(200, RawRefresh);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SettingUtil.SelfPtr.Add(AppUtil.GetHwnd(this));
            AppUtil.ExcludeFromCapture(this);
            AppUtil.DisableAltF4(this);
            AppUtil.AlwaysToTop(this);
            AppUtil.HideAltTab(this);
        }

        public new void Show()
        {
            if (this.IsVisible && currentFlowMode == SettingUtil.GetSetting(SettingUtil.FlowModeKey)) return;
            this.border.Visibility = Visibility.Hidden;
            base.Show();
            this.SizeToContent = SizeToContent.WidthAndHeight;
            InitPos();
        }

        private void RawRefresh()
        {
            Dispatcher.Invoke(() =>
            {
                var allWindow = ToggleWindow.GetAllWindow();
                UIElement? prevItem = null;
                foreach (var item in allWindow.OrderBy((item) => item.Key))
                {
                    prevItem = addWindowItem(item.Key, item.Value, prevItem);
                }

                prevFlowMode = SettingUtil.GetSetting(SettingUtil.FlowModeKey, "0");
                ToggleViewVisible();
            });
        }

        private void ToggleViewVisible()
        {
            if (this.bar.Children.Count > 0)
                this.Show();
            else
                this.Hide();
        }

        private UIElement addWindowItem(Key key, WindowInfo value, UIElement? prevItem)
        {
            var k = key.ToString();
            if (k.StartsWith("NumPad"))
            {
                k = k.Substring(6);
            }
            var flowMode = SettingUtil.GetSetting(SettingUtil.FlowModeKey, "0");
            var name = "kbd_hook_" + k;
            var n = this.bar.FindName(name) as UIElement;
            if (n != null)
            {
                if (prevFlowMode == flowMode)
                {
                    return n;
                }
                else
                {
                    UnregisterName(name);
                    this.bar.Children.Remove(n);
                }
            }

            StackPanel sp = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Vertical,
                Margin = "0" == flowMode ? new Thickness(5, 0, 5, 0) : new Thickness(0, 5, 0, 5)
            };
            RegisterName(name, sp);

            if (value.Icon == null)
            {
                value.Icon = AppUtil.GetAppIcon(value.Pid, value.P);
            }
            var image = new Image
            {
                Width = 18,
                Height = 18,
                Source = value.Icon
            };

            TextBlock tb = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 8,
                Margin = new Thickness(2),
                Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                TextAlignment = TextAlignment.Center,
                Text = k
            };
            sp.MouseLeftButtonDown += (a, e) =>
            {
                ToggleWindow.ToggleWindowToTop(key, value);
            };
            sp.MouseRightButtonDown += (a, e) =>
            {
                ToggleWindow.RemoveKeyWindow(key);
            };
            sp.ToolTip = value.Title;
            sp.Children.Add(image);
            sp.Children.Add(tb);
            if (prevItem != null)
            {
                this.bar.Children.Insert(this.bar.Children.IndexOf(prevItem) + 1, sp);
            }
            else
            {
                this.bar.Children.Add(sp);
            }
            value.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Title")
                {
                    Dispatcher.Invoke(() => sp.ToolTip = value.Title);
                }
                else if (e.PropertyName == "_Remove")
                {
                    Dispatcher.Invoke(() =>
                    {
                        UnregisterName(name);
                        this.bar.Children.Remove(sp);
                        this.ToggleViewVisible();
                    });
                }
            };
            return sp;
        }

        public void Refresh()
        {
            debounce.invoke();
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender != null) this.DragMove();
                var flowMode = SettingUtil.GetSetting(SettingUtil.FlowModeKey, "0");
                if ("0".Equals(flowMode))
                {
                    SettingUtil.SetSetting("bind-view-left", "" + this.Left);
                    this.Top = 0;
                }
                else if ("1".Equals(flowMode))
                {
                    SettingUtil.SetSetting("bind-view-top", "" + this.Top);
                    this.Left = SystemParameters.WorkArea.Width - this.Width;
                }
                else if ("2".Equals(flowMode))
                {
                    SettingUtil.SetSetting("bind-view-top", "" + this.Top);
                    this.Left = 0;
                }
            }
            catch
            {

            }
        }
        private void InitPos()
        {
            var flowMode = SettingUtil.GetSetting(SettingUtil.FlowModeKey, "0");
            if ("0".Equals(flowMode))
            {

                this.Top = 0;
                var left = SettingUtil.GetSetting("bind-view-left");
                if (string.IsNullOrWhiteSpace(left))
                {
                    var width = SystemParameters.WorkArea.Width;
                    this.Left = width / 2 - (this.Width / 2.5);
                }
                else
                {
                    this.Left = Double.Parse(left);
                }
                this.bar.Orientation = Orientation.Horizontal;
                this.border.CornerRadius = new CornerRadius(0, 0, 8, 8);
            }
            else if ("1".Equals(flowMode))
            {
                this.Left = SystemParameters.WorkArea.Width - this.Width;
                var h = SettingUtil.GetSetting("bind-view-top");
                if (string.IsNullOrWhiteSpace(h))
                {
                    var height = SystemParameters.WorkArea.Height;
                    this.Top = height / 2 - (this.Height / 1.5);
                }
                else
                {
                    this.Top = Double.Parse(h);
                }
                this.bar.Orientation = Orientation.Vertical;
                this.border.CornerRadius = new CornerRadius(8, 0, 0, 8);
            }
            else if ("2".Equals(flowMode))
            {

                this.Left = 0;
                var h = SettingUtil.GetSetting("bind-view-top");
                if (string.IsNullOrWhiteSpace(h))
                {
                    var height = SystemParameters.WorkArea.Height;
                    this.Top = height / 2 - (this.Height / 1.5);
                }
                else
                {
                    this.Top = Double.Parse(h);
                }
                this.bar.Orientation = Orientation.Vertical;
                this.border.CornerRadius = new CornerRadius(0, 8, 8, 0);
            }
            this.border.Visibility = Visibility.Visible;
        }
        private new void SizeChangedEvent(object sender, SizeChangedEventArgs e)
        {
            var flowMode = SettingUtil.GetSetting(SettingUtil.FlowModeKey, "0");
            if ("0".Equals(flowMode))
            {
                this.Top = 0;
            }
            else if ("1".Equals(flowMode))
            {
                this.Left = SystemParameters.WorkArea.Width - e.NewSize.Width;
            }
            else if ("2".Equals(flowMode))
            {
                this.Left = 0;
            }
        }
    }
}
