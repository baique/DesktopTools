using DesktopTools.component;
using DesktopTools.views;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace DesktopTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static void RefreshOpacityValue()
        {
            Application.Current.Resources["OpacityValue"] = double.Parse(Setting.GetSetting(Setting.OpacityValueKey, "0.4"));
            ResourceDictionary resource = new ResourceDictionary();
            resource.Source = new Uri("pack://application:,,,/resource/ColorTheme" + Setting.GetSetting(Setting.GlobalThemeKey, "0") + ".xaml");
            Application.Current.Resources.MergedDictionaries[0] = resource;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            var n = Process.GetCurrentProcess();
            foreach (var p in Process.GetProcesses())
            {
                try
                {
                    if (p.ProcessName == n.ProcessName && p.Id != n.Id)
                    {
                        Application.Current.Shutdown();
                        return;
                    }
                }
                catch
                {

                }
            }
            SetSelfStarting(true, "desk_date");
            RefreshOpacityValue();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("出现未知的错误:" + e.Exception.Message);
        }

        /// <summary>
        /// 开机自动启动
        /// </summary>
        /// <param name="started">设置开机启动，或取消开机启动</param>
        /// <param name="exeName">注册表中的名称</param>
        /// <returns>开启或停用是否成功</returns>
        private void SetSelfStarting(bool started, string exeName)
        {
            //设置是否自动启动
            if (started)
            {
                {
                    string path = System.Windows.Forms.Application.ExecutablePath;
                    Microsoft.Win32.RegistryKey rk2 = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Run");
                    rk2.SetValue(exeName, @"""" + path + @"""");
                    rk2.Close();
                }
                {
                    try
                    {
                        string path = System.Windows.Forms.Application.ExecutablePath;
                        Microsoft.Win32.RegistryKey rk2 = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Run");
                        rk2.SetValue(exeName, @"""" + path + @"""");
                        rk2.Close();
                    }
                    catch
                    {

                    }
                }
            }
            else
            {
                {
                    Microsoft.Win32.RegistryKey rk2 = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Run");
                    rk2.DeleteValue(exeName, false);
                    rk2.Close();
                }
                try
                {
                    string path = System.Windows.Forms.Application.ExecutablePath;
                    Microsoft.Win32.RegistryKey rk2 = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Run");
                    rk2.DeleteValue(exeName, false);
                    rk2.Close();
                }
                catch
                {

                }
            }
        }
    }
}
