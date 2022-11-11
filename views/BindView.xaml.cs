﻿using DesktopTools.model;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Image = System.Windows.Controls.Image;
using Color = System.Windows.Media.Color;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using static DesktopTools.util.Win32;
using DesktopTools.views;
using DesktopTools.component;

namespace DesktopTools
{
    /// <summary>
    /// Interaction logic for BindingView.xaml
    /// </summary>
    public partial class BindingView : Window
    {
        public BindingView()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var ptr = new WindowInteropHelper(this).Handle;
            HideAltTab(ptr);
            SetWindowPos(ptr, -1, 0, 0, 0, 0, 3);
            ToggleWindow.addIgnorePtr(this);
        }
        public new void Show()
        {
            base.Show();
            this.Top = 0;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            var left = Setting.GetSetting("bind-view-left");
            if (string.IsNullOrWhiteSpace(left))
            {
                var width = SystemParameters.WorkArea.Width;
                this.Left = width / 2 - (this.Width / 2.5);
            }
            else
            {
                this.Left = Double.Parse(left);
            }
        }
        public void Refresh()
        {
            Dispatcher.InvokeAsync(() =>
            {
                this.bar.Children.Clear();
                int addSize = 0;
                foreach (var item in ToggleWindow.GetAllWindow())
                {
                    try
                    {
                        StackPanel sp = new StackPanel
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Orientation = Orientation.Vertical,
                            Margin = new Thickness(5, 0, 5, 0)
                        };
                        if (item.Value.P.MainModule == null)
                        {
                            throw new Exception("进程模块获取异常");
                        }
                        var fn = item.Value.P.MainModule.FileName;
                        if (fn == null)
                        {
                            throw new Exception("进程模块获取异常");
                        }
                        var icon = System.Drawing.Icon.ExtractAssociatedIcon(fn);

                        Image image = new Image
                        {
                            Height = 18,
                            Width = 18,
                            Source = ToImageSource(icon)
                        };

                        sp.Children.Add(image);

                        TextBlock tb = new TextBlock
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            FontSize = 8,
                            Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                            TextAlignment = TextAlignment.Center,
                            Text = item.Key.ToString(),
                        };
                        sp.MouseLeftButtonDown += (a, e) =>
                        {
                            ToggleWindow.ToggleWindowToTop(item.Key, item.Value);
                        };
                        sp.MouseRightButtonDown += (a, e) =>
                        {
                            ToggleWindow.RemoveKeyWindow(item.Key);
                        };
                        sp.Children.Add(tb);
                        this.bar.Children.Add(sp);
                        addSize++;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }
                this.SizeToContent = SizeToContent.WidthAndHeight;
                if (addSize > 0)
                    this.Show();
                else
                    this.Hide();
            });
        }

        public static ImageSource ToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
                this.Top = 0;
                Setting.SetSetting("bind-view-left", "" + this.Left);
            }
            catch
            {

            }
        }
    }
}
