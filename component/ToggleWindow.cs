using DesktopTools.model;
using DesktopTools.views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using static DesktopTools.component.GlobalKeyboardEvent;
using static DesktopTools.util.Win32;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxOptions = System.Windows.Forms.MessageBoxOptions;

namespace DesktopTools.component
{
    public class ToggleWindow : Event
    {
        private static Dictionary<Keys, WindowInfo> windowBinding = new Dictionary<Keys, WindowInfo>();
        private static List<IntPtr> IgnorePtr = new List<IntPtr>();
        private static BindingView bv = new BindingView();
        public static void addIgnorePtr(Window windowInstance)
        {
            var ptr = new WindowInteropHelper(windowInstance).Handle;
            if (IgnorePtr.Contains(ptr))
            {
                return;
            }
            IgnorePtr.Add(ptr);
        }
        public static bool HasItem()
        {
            return windowBinding.Count > 0;
        }

        public static void ToggleWindowToTop(Keys keyData, WindowInfo info)
        {
            var ptr = info.Ptr;
            var wd = GetForegroundWindow();
            if (IsIconic(ptr) || !IsWindowVisible(ptr))
            {
                ShowWindow(ptr, 9);
                ActiveWindow(keyData, info);

            }
            else if (wd != ptr)
            {
                ActiveWindow(keyData, info);
            }
            else
            {
                ShowWindow(ptr, 2);
            }
        }

        public static Dictionary<Keys, WindowInfo> GetAllWindow()
        {
            return windowBinding;
        }

        private static bool DontContinueBindOther(Keys keyData)
        {
            RemoveKeyWindow(keyData);
            if (MessageBox.Show(
                    "绑定的进程已结束，是否更换到当前前置窗口？",
                    "提示",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.ServiceNotification
                    ) != System.Windows.Forms.DialogResult.Yes)
            {
                return true;
            }
            return false;
        }

        private static void ActiveWindow(Keys keyData, WindowInfo needToTopWindow)
        {
            var fs = GetForegroundWindow();
            var targetInputProcessId = GetWindowThreadProcessId(fs, IntPtr.Zero);
            var curThread = GetCurrentThreadId();
            if (fs == needToTopWindow.Ptr)
            {
                return;
            }

            bool attach = AttachThreadInput(curThread, targetInputProcessId, true);
            try
            {
                SetWindowPos(needToTopWindow.Ptr, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
                SetWindowPos(needToTopWindow.Ptr, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
                if (!SetForegroundWindow(needToTopWindow.Ptr))
                {
                    if (DontContinueBindOther(keyData))
                    {
                        return;
                    }
                    RegisterKeyWindow(keyData, GetForegroundWindow());
                }
            }
            finally
            {
                if (attach)
                {
                    AttachThreadInput(curThread, targetInputProcessId, false);
                }
            }
        }

        public static void ToggleWindowToTop(Keys keyData)
        {
            ToggleWindowToTop(keyData, windowBinding[keyData]);
        }

        public static bool ContainsKey(Keys keyData)
        {
            return windowBinding.ContainsKey(keyData);
        }

        public static void RegisterKeyWindow(Keys keyData, IntPtr wd)
        {
            if (IgnorePtr.Contains(wd))
            {
                return;
            }

            int pid = 0;
            GetWindowThreadProcessId(wd, out pid);
            Process proc = Process.GetProcessById(pid);
            var wi = new WindowInfo
            (
                GetText(wd),
                wd,
                proc
            );
            if (String.IsNullOrWhiteSpace(wi.Title))
            {
                if (proc != null)
                {
                    if (proc.ProcessName != null)
                    {
                        wi.Title = proc.ProcessName;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(wi.Title))
            {
                return;
            }
            if (windowBinding.ContainsValue(wi))
            {
                if (MessageBox.Show(
                    "[" + wi.Title + "]已经绑定了一个快捷键，是否仍要绑定？",
                    "提示",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.ServiceNotification
                    ) != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }
            }
            windowBinding[keyData] = wi;
            bv.Refresh();
        }

        public static void RemoveKeyWindow()
        {
            var w = GetForegroundWindow();
            foreach (var item in windowBinding)
            {
                if (item.Value.Ptr == w)
                {
                    windowBinding.Remove(item.Key);
                }
            }
            bv.Refresh();
        }

        public static void RemoveKeyWindow(Keys key)
        {
            windowBinding.Remove(key);
            bv.Refresh();
        }


        public static string GetText(IntPtr hWnd)
        {
            // Allocate correct string length first
            int length = GetWindowTextLength(hWnd);
            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        public string Key()
        {
            return Setting.GetSetting(Setting.ForceWindowBindOrChangeKey, "LeftCtrl + LeftAlt");
        }


        public void Handler(KeyEventArgs e)
        {
            if ((e.KeyValue >= (int)Keys.NumPad0 && e.KeyValue <= (int)Keys.NumPad9) || e.KeyValue >= (int)Keys.D0 && e.KeyValue <= (int)Keys.D9)
            {
                RegisterKeyWindow(e.KeyData, GetForegroundWindow());
            }
        }

        public static void ToggleIconPanel()
        {
            if (windowBinding.Count > 0)
            {
                try
                {
                    if (!bv.IsVisible)
                    {
                        bv.Show();
                    }
                }
                catch
                {
                    bv = new BindingView();
                    bv.Show();
                }
            }
            else
            {
                try
                {
                    if (bv.IsVisible)
                    {
                        bv.Hide();
                    }
                }
                catch
                {
                }
            }
        }
    }
}
