using DesktopTools.util;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace DesktopTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static void RefreshOpacityValue()
        {
            ResourceDictionary resource = new ResourceDictionary();
            resource.Source = new Uri("pack://application:,,,/resource/ColorTheme" + SettingUtil.GetSetting(SettingUtil.GlobalThemeKey, "0") + ".xaml");
            Application.Current.Resources.MergedDictionaries[0] = resource;
            Application.Current.Resources["OpacityValue"] = double.Parse(SettingUtil.GetSetting(SettingUtil.OpacityValueKey, "0.4"));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var proc = GetRunningInstance();
            if (proc != null)
            {
                HandleRunningInstance(proc);
                Application.Current.Shutdown();
                return;
            }
            // 历史版本错误的启动项移除
            SetSelfStarting(false, "desk_date");
            // 历史版本错误的启动项移除
            SetSelfStarting(false, "DesktopTools", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            SetSelfStarting(true, "DesktopTools", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            RefreshOpacityValue();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

  

        /// <summary>
        /// 获取当前是否具有相同进程。
        /// </summary>
        /// <returns></returns>
        public static Process GetRunningInstance()
        {
            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(current.ProcessName);
            //遍历正在有相同名字运行的例程   
            foreach (Process process in processes)
            {
                //忽略现有的例程   
                if (process.Id != current.Id)
                {
                    //确保例程从EXE文件运行 
                    if (process.ProcessName == process.ProcessName)
                        return process;
                }
            }
            return null;
        }

        private const int WS_SHOWNORMAL = 1;
        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        /// <summary>
        /// 激活原有的进程。
        /// </summary>
        /// <param name="instance"></param>
        public static void HandleRunningInstance(Process instance)
        {
            ShowWindowAsync(instance.MainWindowHandle, WS_SHOWNORMAL);
            SetForegroundWindow(instance.MainWindowHandle);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exp = e.ExceptionObject as Exception;
            if (exp != null)
            {
#if DEBUG
                File.AppendAllText("error.log", exp.Message + "\r\n" + exp.StackTrace);
#endif
                MessageBox.Show("预期外的错误:" + exp.Message);
            }
            else
                MessageBox.Show("预期外的错误");
            App.Current.Shutdown(1);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            var exp = e.Exception;
#if DEBUG
            File.AppendAllText("error.log", exp.Message + "\r\n" + exp.StackTrace);
#endif
            MessageBox.Show("预期外的错误:" + exp.Message);
            App.Current.Shutdown(1);
        }

        /// <summary>
        /// 开机自动启动
        /// </summary>
        /// <param name="started">设置开机启动，或取消开机启动</param>
        /// <param name="exeName">注册表中的名称</param>
        /// <returns>开启或停用是否成功</returns>
        private void SetSelfStarting(bool started, string exeName, string key = @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Run")
        {
            //设置是否自动启动
            if (started)
            {
                try
                {
                    string path = System.Windows.Forms.Application.ExecutablePath;
                    Microsoft.Win32.RegistryKey rk2 = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(key);
                    rk2.SetValue(exeName, @"""" + path + @"""");
                    rk2.Close();
                }
                catch
                {
                    {
                        string path = System.Windows.Forms.Application.ExecutablePath;
                        Microsoft.Win32.RegistryKey rk2 = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(key);
                        rk2.SetValue(exeName, @"""" + path + @"""");
                        rk2.Close();
                    }
                }
            }
            else
            {
                {
                    Microsoft.Win32.RegistryKey rk2 = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(key);
                    rk2.DeleteValue(exeName, false);
                    rk2.Close();
                }
                try
                {
                    Microsoft.Win32.RegistryKey rk2 = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(key);
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
