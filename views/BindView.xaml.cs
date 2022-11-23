using DesktopTools.component;
using DesktopTools.util;
using DesktopTools.views;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static DesktopTools.util.Win32;
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
        public BindingView()
        {
            InitializeComponent();
            debounce = new Debounce(150, RawRefresh);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ToggleWindow.addIgnorePtr(this);
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
            Dispatcher.InvokeAsync(() =>
            {
                this.bar.Children.Clear();
                int addSize = 0;
                var allWindow = ToggleWindow.GetAllWindow();
                var flowMode = SettingUtil.GetSetting(SettingUtil.FlowModeKey, "0");
                foreach (var item in allWindow.OrderBy((item) => item.Key))
                {
                    StackPanel sp = new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Orientation = Orientation.Vertical,
                        Margin = "0" == flowMode ? new Thickness(5, 0, 5, 0) : new Thickness(0, 5, 0, 5)
                    };

                    Image image = new Image
                    {
                        Height = 18,
                        Width = 18,
                        Source = item.Value.Icon
                    };


                    var k = item.Key.ToString();
                    if (k.StartsWith("NumPad"))
                    {
                        k = k.Substring(6);
                    }
                    TextBlock tb = new TextBlock
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 8,
                        Margin = new Thickness(2),
                        Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                        TextAlignment = TextAlignment.Center,
                        Text = k,
                    };
                    sp.MouseLeftButtonDown += (a, e) =>
                    {
                        ToggleWindow.ToggleWindowToTop(item.Key, item.Value);
                    };
                    sp.MouseRightButtonDown += (a, e) =>
                    {
                        ToggleWindow.RemoveKeyWindow(item.Key);
                    };
                    sp.ToolTip = item.Value.Title;
                    sp.Children.Add(image);
                    sp.Children.Add(tb);
                    this.bar.Children.Add(sp);
                    addSize++;
                }
                if (addSize > 0)
                    this.Show();
                else
                    this.Hide();
            });
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
