using BeanFramework.core.bean;
using DesktopTools.component.model;
using DesktopTools.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using static DesktopTools.util.Win32;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxOptions = System.Windows.Forms.MessageBoxOptions;

namespace DesktopTools.component
{
    [Bean(Name = "窗体切换实例")]
    public class ToggleWindow : Component
    {
        private static object doubleWriteLock = new object();
        private static Dictionary<Key, WindowInfo> windowBinding = new Dictionary<Key, WindowInfo>();
        private static Dictionary<IntPtr, List<Key>> windowBindingIndex = new Dictionary<IntPtr, List<Key>>();
        private static CancellationTokenSource cts = new CancellationTokenSource();
        private static Dictionary<Key, WindowInfo> waitFlowWindowList = new Dictionary<Key, WindowInfo>();

        public static Dictionary<Key, WindowInfo> GetAllWindow()
        {
            return windowBinding;
        }
        private static BindingView bv;

        public static bool ContainsKey(Key keyData)
        {
            return windowBinding.ContainsKey(keyData);
        }

        public static void TryFlowWindow(IntPtr hwnd)
        {
            if (windowBindingIndex.ContainsKey(hwnd))
            {
                var item = windowBindingIndex[hwnd][0];
                var winInfo = windowBinding[item];
                RemoveKeyWindow(windowBindingIndex[hwnd].ToArray());
                winInfo.CloseTime = DateTime.Now;
                Monitor.Enter(waitFlowWindowList);
                try
                {
                    waitFlowWindowList[item] = winInfo;
                    Monitor.Pulse(waitFlowWindowList);
                }
                finally
                {
                    Monitor.Exit(waitFlowWindowList);
                }
            }
        }

        public static void Rename(IntPtr hwnd)
        {
            if (windowBindingIndex.ContainsKey(hwnd))
            {
                var o = windowBinding[windowBindingIndex[hwnd][0]];
                o.LoadTitle();
            }
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

        public static void ToggleWindowToTop(Key keyData)
        {
            ToggleWindowToTop(keyData, windowBinding[keyData]);
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
            if (SettingUtil.SelfPtr.Contains(wd)) return;
            try
            {
                int pid = 0;
                GetWindowThreadProcessId(wd, out pid);

                if (pid == 0) throw new Exception("进程模块获取异常");

                Process proc = Process.GetProcessById(pid);
                if (proc == null) throw new Exception("进程模块获取异常");

                var wi = new WindowInfo(wd, pid, proc);

                if (string.IsNullOrWhiteSpace(wi.Title) || wi.Title == "Program Manager" || wi.Title == "explorer") return;
                if (!ignoreError && windowBinding.ContainsValue(wi))
                    MainWindow.Notify.ShowBalloonTip(100, "提示", "[" + wi.Title + "]的快捷键被重复绑定", ToolTipIcon.Warning);
                lock (doubleWriteLock)
                {
                    windowBinding[keyData] = wi;
                    if (!windowBindingIndex.ContainsKey(wi.Ptr)) windowBindingIndex[wi.Ptr] = new List<Key>();
                    windowBindingIndex[wi.Ptr].Add(keyData);
                    AddWindowToReg(keyData, wi.Ptr);

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
                    item.Value.OnPropertyChanged("_Remove");
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
                    item.OnPropertyChanged("_Remove");
                    windowBindingIndex[e].Remove(key);
                    if (windowBindingIndex[e].Count == 0) windowBindingIndex.Remove(e);
                    RemoveWindowFromReg(key, e);
                }
            }

            bv.Refresh();
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
                SetWindowPos(needToTopWindow.Ptr, 0, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
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

        private static void AddWindowToReg(Key keyData, IntPtr ptr)
        {
            var currentBinding = SettingUtil.GetSetting("last-binding-key-window");
            SettingUtil.SetSetting("last-binding-key-window", string.Join(";", currentBinding, keyData.ToString() + ":" + ptr.ToInt64()));
            SettingUtil.SetSetting("last-sys-update-time", Environment.TickCount64.ToString());
        }

        private static void RemoveWindowFromReg(Key key, IntPtr ptr, IntPtr? newPtr = null)
        {
            var currentBinding = SettingUtil.GetSetting("last-binding-key-window");
            if (newPtr != null)
            {
                SettingUtil.SetSetting("last-binding-key-window", currentBinding.Replace(key.ToString() + ":" + ptr.ToInt64(), key.ToString() + ":" + newPtr.Value.ToInt64()));
            }
            else
            {
                SettingUtil.SetSetting("last-binding-key-window", currentBinding.Replace(key.ToString() + ":" + ptr.ToInt64(), ""));
            }
            SettingUtil.SetSetting("last-sys-update-time", Environment.TickCount64.ToString());
        }

        public void Init()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                bv = new BindingView();
                RestoreKeyWindow();
            });
            Task.Run(() =>
            {
                while (!cts.IsCancellationRequested)
                {
                    Monitor.Enter(waitFlowWindowList);
                    try
                    {
                        if (waitFlowWindowList.Count == 0)
                        {
                            Monitor.Wait(waitFlowWindowList);
                        }
                        foreach (var winInfo in waitFlowWindowList)
                        {
                            if (winInfo.Value.P.HasExited)
                            {
                                waitFlowWindowList.Remove(winInfo.Key);
                                continue;
                            }
                            if (DateTime.Now.Subtract(winInfo.Value.CloseTime).TotalSeconds > 5)
                            {
                                waitFlowWindowList.Remove(winInfo.Key);
                                continue;
                            }
                            lock (doubleWriteLock)
                            {
                                if (windowBinding.ContainsKey(winInfo.Key))
                                {
                                    waitFlowWindowList.Remove(winInfo.Key);
                                    continue;
                                }
                                var newTargetPtr = winInfo.Value.P.MainWindowHandle;

                                if (newTargetPtr != IntPtr.Zero && !windowBindingIndex.ContainsKey(newTargetPtr))
                                {
                                    RegisterKeyWindow(winInfo.Key, newTargetPtr);
                                    waitFlowWindowList.Remove(winInfo.Key);
                                    continue;
                                }
                            }
                        }
                    }
                    catch
                    {

                    }
                    finally
                    {
                        Monitor.Exit(waitFlowWindowList);
                    }
                }
            });
        }

        public void Destroy()
        {
            cts.Cancel();
        }
    }
}
