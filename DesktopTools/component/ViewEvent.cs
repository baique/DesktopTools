using BeanFramework.core.bean;
using DesktopTools.util;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using static DesktopTools.util.Win32;

namespace DesktopTools.component
{
    [Bean(Name = "监听并执行变更事件")]
    public class ViewEvent : Component
    {
        private class ProcEvent { public uint T { get; set; } public IntPtr H { get; set; } }
        private static ConcurrentQueue<ProcEvent> EventQueue = new ConcurrentQueue<ProcEvent>();
        private static CancellationTokenSource cts = new CancellationTokenSource();
        private static Debounce refreshTopmost = new Debounce(1500, () =>
        {
            foreach (var ptr in SettingUtil.SelfPtr) SetWindowPos(ptr, -1, 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0004 | 0x0020 | 0x0040);
        });

        private static void OnProcMsg(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hwnd == IntPtr.Zero) return;
            if (idObject != 0 || idChild != 0) return;
            EventQueue.Enqueue(new ProcEvent
            {
                T = eventType,
                H = hwnd,
            });
        }

        public void Init()
        {
            Task.Run(() =>
            {
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        ProcEvent? d;
                        if (!EventQueue.TryDequeue(out d))
                        {
                            Thread.Sleep(100);
                            continue;
                        };

                        var eventType = d.T;
                        var hwnd = d.H;
                        if (eventType == 0x8001)
                        {
                            ToggleWindow.TryFlowWindow(hwnd);
                        }
                        else if (eventType == 0x800C)
                        {
                            ToggleWindow.Rename(hwnd);
                        }
                        else if (eventType == 0x0003)
                        {
                            if (SettingUtil.SelfPtr.Contains(hwnd))
                            {
                                continue;
                            }
                            refreshTopmost.invoke();
                        }

                    }
                    catch { }
                }
            });
        }

        public void Destroy()
        {
            cts.Cancel();
        }

        [Bean(Name = "监听窗口名称变更")]
        public class BindViewRenameEvent : Component
        {
            private IntPtr nameChangedEventHook;
            private static WinEventDelegate onProc = new WinEventDelegate(OnProcMsg);

            public void Init()
            {
                nameChangedEventHook = SetWinEventHook(
                        WinEvents.EVENT_OBJECT_NAMECHANGE, WinEvents.EVENT_OBJECT_NAMECHANGE,
                        AppUtil.GetHwnd(System.Windows.Application.Current.MainWindow),
                        onProc, 0, 0,
                        WinEventFlags.WINEVENT_OUTOFCONTEXT | WinEventFlags.WINEVENT_SKIPOWNPROCESS
                    );
            }

            public void Destroy()
            {
                UnhookWinEvent(nameChangedEventHook);
            }
        }

        [Bean(Name = "前景窗口切换事件")]
        public class ViewToggleEvent : Component
        {
            private IntPtr hook;
            private static WinEventDelegate onProc = new WinEventDelegate(OnProcMsg);

            public void Init()
            {
                hook = SetWinEventHook(
                        WinEvents.EVENT_SYSTEM_FOREGROUND, WinEvents.EVENT_SYSTEM_FOREGROUND,
                        IntPtr.Zero,
                        onProc, 0, 0,
                        WinEventFlags.WINEVENT_OUTOFCONTEXT | WinEventFlags.WINEVENT_SKIPOWNPROCESS
                    );
            }

            public void Destroy()
            {
                UnhookWinEvent(hook);
            }
        }

        [Bean(Name = "窗口注销事件")]
        public class BindViewDestroyEvent : Component
        {
            private static IntPtr destroyEventHook;
            private static WinEventDelegate onProc = new WinEventDelegate(OnProcMsg);

            public void Init()
            {
                destroyEventHook = SetWinEventHook(
                        WinEvents.EVENT_OBJECT_DESTROY, WinEvents.EVENT_OBJECT_DESTROY,
                        IntPtr.Zero,
                        onProc, 0, 0,
                        WinEventFlags.WINEVENT_OUTOFCONTEXT | WinEventFlags.WINEVENT_SKIPOWNPROCESS
                    );
            }

            public void Destroy()
            {
                UnhookWinEvent(destroyEventHook);
            }
        }
    }

}
