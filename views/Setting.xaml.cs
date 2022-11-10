using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TextBox = System.Windows.Controls.TextBox;

namespace DesktopTools.views
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Setting : Window
    {
        public Setting()
        {
            InitializeComponent();
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch
            {

            }
        }

        internal static bool SetSetting(string key, string value)
        {
            Microsoft.Win32.RegistryKey rk2 = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\desktop_tools");
            try
            {
                rk2.SetValue(key, value);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                rk2.Close();
            }
        }

        internal static string GetSetting(string key, string def = "")
        {
            Microsoft.Win32.RegistryKey rk2 = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\desktop_tools");
            try
            {
                var val = "";
                var v = rk2.GetValue(key);
                if (v != null)
                {
                    val = v.ToString();
                }
                if (string.IsNullOrEmpty(val))
                {
                    return def;
                }
                return val;
            }
            catch
            {
                return def;
            }
            finally
            {
                rk2.Close();
            }
        }

        internal static string EnableBiYingKey = "enable-biying";
        internal static string ChangeBiYingBackgroundKey = "change-biying-background";
        internal static string WindowBindOrChangeKey = "window-bind-change";
        internal static string ForceWindowBindOrChangeKey = "force-window-bind-change";
        internal static string UnWindowBindOrChangeKey = "un-window-bind-change";
        internal static string ErrorModeKey = "error-mode";
        internal static string EnableGoodbyeModeKey = "enable-goodbye";
        internal static string EnableGoodbyeMKey = "enable-goodbye-m";
        internal static string EnableGoodbyeHKey = "enable-goodbye-h";
        internal static string EnableMouseGoodbyeModeKey = "enable-mouse-goodbye";
        internal static string EnableDisableLockScreenKey = "enable-disable-lock-screen";
        internal static string ChangeEnableDisableLockScreenKey = "global-change-enable-disable-lock-screen";

        private void WinLoaded(object sender, RoutedEventArgs e)
        {
            this.EnableBiYingBackground.IsChecked = "1".Equals(GetSetting(EnableBiYingKey));
            this.ChangeBiYingBackground.Text = GetSetting(ChangeBiYingBackgroundKey, "LeftCtrl + LeftAlt + B + N");
            this.WindowBindOrChange.Text = GetSetting(WindowBindOrChangeKey, "LeftCtrl");
            this.ForceWindowBindOrChange.Text = GetSetting(ForceWindowBindOrChangeKey, "LeftCtrl + LeftAlt");
            this.UnWindowBindOrChange.Text = GetSetting(UnWindowBindOrChangeKey, "LeftCtrl + LeftAlt + Back");
            this.ErrorMode.Text = GetSetting(ErrorModeKey, "LeftCtrl + LeftShift + Space");
            this.EnableGoodbyeMode.IsChecked = "1".Equals(GetSetting(EnableGoodbyeModeKey));
            //this.EnableMouseGoodbyeMode.IsChecked = "1".Equals(GetSetting(EnableMouseGoodbyeModeKey));
            this.EnableDisableLockScreen.IsChecked = "1".Equals(GetSetting(EnableDisableLockScreenKey, "1"));
            this.EnableGoodbyeH.Text = GetSetting(EnableGoodbyeHKey, "17");
            this.EnableGoodbyeM.Text = GetSetting(EnableGoodbyeMKey, "30");
            this.ChangeEnableDisableLockScreen.Text = GetSetting(ChangeEnableDisableLockScreenKey, "LeftCtrl + LeftAlt + Space");
        }

        private void SaveChange(object sender, RoutedEventArgs e)
        {
            SetSetting(EnableBiYingKey, this.EnableBiYingBackground.IsChecked.Value ? "1" : "0");
            SetSetting(ChangeBiYingBackgroundKey, this.ChangeBiYingBackground.Text);
            SetSetting(WindowBindOrChangeKey, this.WindowBindOrChange.Text);
            SetSetting(ForceWindowBindOrChangeKey, this.ForceWindowBindOrChange.Text);
            SetSetting(UnWindowBindOrChangeKey, this.UnWindowBindOrChange.Text);
            SetSetting(ErrorModeKey, this.ErrorMode.Text);
            SetSetting(ErrorModeKey, this.ErrorMode.Text);
            SetSetting(EnableGoodbyeHKey, this.EnableGoodbyeH.Text);
            SetSetting(EnableGoodbyeMKey, this.EnableGoodbyeM.Text);
            SetSetting(EnableGoodbyeModeKey, this.EnableGoodbyeMode.IsChecked.Value ? "1" : "0");
            //SetSetting(EnableMouseGoodbyeModeKey, this.EnableMouseGoodbyeMode.IsChecked.Value ? "1" : "0");
            SetSetting(EnableDisableLockScreenKey, this.EnableDisableLockScreen.IsChecked.Value ? "1" : "0");
            SetSetting(ChangeEnableDisableLockScreenKey, this.ChangeEnableDisableLockScreen.Text);
            MainWindow.Notify.ShowBalloonTip(300, "修改成功", "新的配置已被应用", ToolTipIcon.Info);
            this.Close();
        }

        private void KeyBind(object sender, KeyEventArgs e)
        {
            List<Key> keys = new List<Key>();
            foreach (Key item in Enum.GetValues(typeof(Key)))
            {
                try
                {
                    if (Keyboard.IsKeyDown(item))
                    {
                        keys.Add(item);
                    }
                }
                catch
                {

                }
            }
            ((TextBox)sender).Text = String.Join(" + ", keys.Select(f => f.ToString()).ToArray());
        }
    }
}
