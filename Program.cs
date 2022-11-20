using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DesktopTools
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var proc = GetRunningInstance();
            if (proc != null)
            {
                HandleRunningInstance(proc);
                return;
            }
            var application = new App();
            application.InitializeComponent();
            application.Run();
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
                    //确保例程从EXE文件运行 
                    if (process.ProcessName == process.ProcessName)
                        return process;
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
    }
}
