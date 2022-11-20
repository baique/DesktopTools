using DesktopTools.component.model;
using DesktopTools.util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using static DesktopTools.util.Win32;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxOptions = System.Windows.Forms.MessageBoxOptions;

namespace DesktopTools.component
{
    public class ToggleWindow
    {
        private static object doubleWriteLock = new object();
        private static Dictionary<Key, WindowInfo> windowBinding = new Dictionary<Key, WindowInfo>();
        private static Dictionary<IntPtr, List<Key>> windowBindingIndex = new Dictionary<IntPtr, List<Key>>();
        private static List<IntPtr> IgnorePtr = new List<IntPtr>();
        private static BindingView bv = new BindingView();
        private static WinEventDelegate onProc = new WinEventDelegate(OnProcMsg);
        private static APPBARDATA abd;
        private static int uCallBackMsg;
        private static ConcurrentQueue<ProcEvent> EventQueue = new ConcurrentQueue<ProcEvent>();

        private static CancellationTokenSource cts = new CancellationTokenSource();
        private static IntPtr destroyEventHook, nameChangedEventHook;
        public static void Register()
        {
            abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = new WindowInteropHelper(App.Current.MainWindow).Handle;
            uCallBackMsg = RegisterWindowMessage("APPBARMSG_CSDN_HELPER");
            abd.uCallbackMessage = uCallBackMsg;
            SHAppBarMessage((int)ABMsg.ABM_NEW, ref abd);
            HwndSource source = HwndSource.FromHwnd(abd.hWnd);
            source.AddHook(new HwndSourceHook(ForegoundChangeProcMsg));

            destroyEventHook = SetWinEventHook(
                        WinEvents.EVENT_OBJECT_DESTROY, WinEvents.EVENT_OBJECT_DESTROY,
                        abd.hWnd,
                        onProc, 0, 0,
                        WinEventFlags.WINEVENT_OUTOFCONTEXT | WinEventFlags.WINEVENT_SKIPOWNPROCESS
                    );
            if (destroyEventHook == IntPtr.Zero) throw new Exception("事件监听注册失败！");

            nameChangedEventHook = SetWinEventHook(
                WinEvents.EVENT_OBJECT_NAMECHANGE, WinEvents.EVENT_OBJECT_NAMECHANGE,
                abd.hWnd,
                onProc, 0, 0,
                WinEventFlags.WINEVENT_OUTOFCONTEXT | WinEventFlags.WINEVENT_SKIPOWNPROCESS
            );
            if (nameChangedEventHook == IntPtr.Zero)
            {
                UnhookWinEvent(destroyEventHook);
                throw new Exception("事件监听注册失败！");
            }
            Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        ProcEvent? d;
                        if (!EventQueue.TryDequeue(out d))
                        {
                            await Task.Delay(10);
                            continue;
                        };

                        var eventType = d.T;
                        var hwnd = d.H;
                        if (eventType == 0x8001)
                        {
                            if (windowBindingIndex.ContainsKey(hwnd)) RemoveKeyWindow(windowBindingIndex[hwnd].ToArray());
                        }
                        else if (eventType == 0x800C)
                        {
                            if (windowBindingIndex.ContainsKey(hwnd))
                            {
                                lock (doubleWriteLock)
                                {
                                    var newText = GetText(hwnd);
                                    var item = windowBinding[windowBindingIndex[hwnd][0]];
                                    if (newText.Equals(item.Title)) continue;
                                    item.Title = newText;
                                }
                                bv.Refresh();
                            }
                        }
                    }
                    catch { }
                }

            });
        }
        public static void Close()
        {
            try { UnhookWinEvent(destroyEventHook); } catch { }
            try { UnhookWinEvent(nameChangedEventHook); } catch { }
            cts.Cancel();
            SHAppBarMessage((int)ABMsg.ABM_REMOVE, ref abd);
        }


        public static IntPtr ForegoundChangeProcMsg(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == uCallBackMsg && wParam.ToInt32() == (int)ABNotify.ABN_FULLSCREENAPP)
            {
                if ((int)lParam == 1) SettingUtil.HasFullScreen = true;
                else SettingUtil.HasFullScreen = false;
            }
            return IntPtr.Zero;
        }


        class ProcEvent { public uint T { get; set; } public IntPtr H { get; set; } }
        private static void OnProcMsg(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hwnd == IntPtr.Zero) return;
            if (idObject != 0 || idChild != 0) return;
#if DEBUG
            Trace.WriteLine("event:" + hwnd + "-" + eventType);
#endif
            EventQueue.Enqueue(new ProcEvent
            {
                T = eventType,
                H = hwnd,
            });
        }

        public static void addIgnorePtr(Window windowInstance)
        {
            var ptr = new WindowInteropHelper(windowInstance).Handle;
            if (IgnorePtr.Contains(ptr)) return;
            IgnorePtr.Add(ptr);
        }

        public static bool HasItem()
        {
            return windowBinding.Count > 0;
        }

        public static void ToggleWindowToTop(Key keyData, WindowInfo info)
        {
            var ptr = info.Ptr;
            var wd = GetForegroundWindow();
            if (IsIconic(ptr) || !IsWindowVisible(ptr))
            {
                ShowWindow(ptr, 9);
                ActiveWindow(keyData, info);
            }
            else if (wd != ptr) ActiveWindow(keyData, info);
            else ShowWindow(ptr, 6);
        }

        public static Dictionary<Key, WindowInfo> GetAllWindow()
        {
            return windowBinding;
        }

        private static bool DontContinueBindOther(Key keyData)
        {
            RemoveKeyWindow(keyData);
            if (MessageBox.Show(
                    "绑定的进程已结束，是否更换到当前前置窗口？",
                    "提示",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.ServiceNotification
                    ) != DialogResult.Yes)
            {
                return true;
            }
            return false;
        }

        private static void ActiveWindow(Key keyData, WindowInfo needToTopWindow)
        {
            var fs = GetForegroundWindow();
            var targetInputProcessId = GetWindowThreadProcessId(fs, IntPtr.Zero);
            var curThread = (uint)GetCurrentThreadId();
            if (fs == needToTopWindow.Ptr) return;

            bool attach = AttachThreadInput(curThread, targetInputProcessId, true);
            try
            {
                SetWindowPos(needToTopWindow.Ptr, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
                SetWindowPos(needToTopWindow.Ptr, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
                if (!SetForegroundWindow(needToTopWindow.Ptr))
                {
                    try
                    {
                        if (needToTopWindow.P != null && needToTopWindow.P.HasExited) if (DontContinueBindOther(keyData)) return;
                    }
                    catch { return; }
                    RegisterKeyWindow(keyData, GetForegroundWindow());
                }
            }
            finally
            {
                if (attach) AttachThreadInput(curThread, targetInputProcessId, false);
            }
        }

        public static void ToggleWindowToTop(Key keyData)
        {
            ToggleWindowToTop(keyData, windowBinding[keyData]);
        }

        public static bool ContainsKey(Key keyData)
        {
            return windowBinding.ContainsKey(keyData);
        }

        public static void RestoreKeyWindow()
        {
            var lastSysUpdateTime = SettingUtil.GetSetting("last-sys-update-time", "0");

            var currentBinding = SettingUtil.GetSetting("last-binding-key-window");

            SettingUtil.SetSetting("last-binding-key-window", "");
            SettingUtil.SetSetting("last-sys-update-time", Environment.TickCount64.ToString());
            if (long.Parse(lastSysUpdateTime) > Environment.TickCount64) return;
            var bindItems = currentBinding.Split(";");
            foreach (var b in bindItems)
            {
                if (string.IsNullOrWhiteSpace(b)) continue;
                var keyAndData = b.Split(":");
                Key k = (Key)Enum.Parse(typeof(Key), keyAndData[0]);
                RegisterKeyWindow(k, new IntPtr(long.Parse(keyAndData[1])), true);
            }
        }

        public static void RegisterKeyWindow(Key keyData, IntPtr wd, bool ignoreError = false)
        {
            if (IgnorePtr.Contains(wd)) return;
            try
            {
                int pid = 0;
                GetWindowThreadProcessId(wd, out pid);

                if (pid == 0) throw new Exception("进程模块获取异常");

                var title = GetText(wd);
                Process proc = Process.GetProcessById(pid);
                if (proc == null) throw new Exception("进程模块获取异常");

                if (String.IsNullOrWhiteSpace(title)) if (proc.ProcessName != null) title = proc.ProcessName;

                if (string.IsNullOrWhiteSpace(title) || title == "Program Manager" || title == "explorer") return;

                var wi = new WindowInfo(title, wd, pid, proc, AppUtil.GetAppIcon(pid, proc));
                if (!ignoreError && windowBinding.ContainsValue(wi))
                    MainWindow.Notify.ShowBalloonTip(100, "提示", "[" + wi.Title + "]的快捷键被重复绑定", ToolTipIcon.Warning);

                lock (doubleWriteLock)
                {
#if DEBUG
                    Trace.WriteLine("注册成功" + wi.Ptr);
#endif
                    windowBinding[keyData] = wi;
                    if (!windowBindingIndex.ContainsKey(wi.Ptr)) windowBindingIndex[wi.Ptr] = new List<Key>();
                    windowBindingIndex[wi.Ptr].Add(keyData);
                    var currentBinding = SettingUtil.GetSetting("last-binding-key-window");
                    SettingUtil.SetSetting("last-binding-key-window", String.Join(";", currentBinding, keyData.ToString() + ":" + wi.Ptr.ToInt64()));
                    SettingUtil.SetSetting("last-sys-update-time", Environment.TickCount64.ToString());
                }
                bv.Refresh();
            }
            catch
            {
                if (!ignoreError)
                {
                    throw;
                }
            }
        }

        public static void RemoveKeyWindow()
        {
            var w = GetForegroundWindow();
            foreach (var item in new Dictionary<Key, WindowInfo>(windowBinding))
            {
                if (item.Value.Ptr != w) continue;
                lock (doubleWriteLock)
                {
                    windowBinding.Remove(item.Key);
                    var currentBinding = SettingUtil.GetSetting("last-binding-key-window");
                    foreach (Key d in windowBindingIndex[w]) currentBinding = currentBinding.Replace(d.ToString() + ":" + w.ToInt64(), "");
                    SettingUtil.SetSetting("last-binding-key-window", currentBinding);
                    SettingUtil.SetSetting("last-sys-update-time", Environment.TickCount64.ToString());
                    windowBindingIndex.Remove(w);
                }
            }
            bv.Refresh();
        }

        public static void RemoveKeyWindow(params Key[] keys)
        {
            foreach (var key in keys)
            {
                var item = windowBinding[key];
                var e = item.Ptr;
                lock (doubleWriteLock)
                {
                    windowBinding.Remove(key);
                    windowBindingIndex[e].Remove(key);
                    if (windowBindingIndex[e].Count == 0) windowBindingIndex.Remove(e);
                    var currentBinding = SettingUtil.GetSetting("last-binding-key-window");
                    SettingUtil.SetSetting("last-binding-key-window", currentBinding.Replace(key.ToString() + ":" + e, ""));
                    SettingUtil.SetSetting("last-sys-update-time", Environment.TickCount64.ToString());
                    foreach (var h in item.Hook) try { UnhookWinEvent(h); } catch { }
                }
            }

            bv.Refresh();
        }


        public static string GetText(IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }


        public static void ToggleIconPanel()
        {
            if (windowBinding.Count > 0)
            {
                try { if (!bv.IsVisible) bv.Show(); } catch { bv = new BindingView(); bv.Show(); }
            }
            else
            {
                try { if (bv.IsVisible) bv.Hide(); } catch { }
            }
        }

        public static BindingView IconPanel()
        {
            return bv;
        }
    }
}
