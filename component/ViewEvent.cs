using DesktopTools.component.impl;
using DesktopTools.component.support;
using DesktopTools.util;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using static DesktopTools.util.Win32;

namespace DesktopTools.component
{
    public class ViewEvent : EventTrigger<MainWindow, bool>, ResourceHook
    {
        class ProcEvent { public uint T { get; set; } public IntPtr H { get; set; } }
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

        public void Register()
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

        public void UnRegister()
        {
            cts.Cancel();
        }

        public class BindViewRenameEvent : EventTrigger<MainWindow, bool>, ResourceHook
        {
            private IntPtr nameChangedEventHook;
            private static WinEventDelegate onProc = new WinEventDelegate(OnProcMsg);

            public void Register()
            {
                nameChangedEventHook = SetWinEventHook(
                        WinEvents.EVENT_OBJECT_NAMECHANGE, WinEvents.EVENT_OBJECT_NAMECHANGE,
                        AppUtil.GetHwnd(System.Windows.Application.Current.MainWindow),
                        onProc, 0, 0,
                        WinEventFlags.WINEVENT_OUTOFCONTEXT | WinEventFlags.WINEVENT_SKIPOWNPROCESS
                    );
            }

            public void UnRegister()
            {
                UnhookWinEvent(nameChangedEventHook);
            }
        }

        public class ViewToggleEvent : EventTrigger<MainWindow, bool>, ResourceHook
        {
            private IntPtr hook;
            private static WinEventDelegate onProc = new WinEventDelegate(OnProcMsg);

            public void Register()
            {
                hook = SetWinEventHook(
                        WinEvents.EVENT_SYSTEM_FOREGROUND, WinEvents.EVENT_SYSTEM_FOREGROUND,
                        IntPtr.Zero,
                        onProc, 0, 0,
                        WinEventFlags.WINEVENT_OUTOFCONTEXT | WinEventFlags.WINEVENT_SKIPOWNPROCESS
                    );
            }

            public void UnRegister()
            {
                UnhookWinEvent(hook);
            }
        }

        public class BindViewDestroyEvent : EventTrigger<MainWindow, bool>, ResourceHook
        {
            private static IntPtr destroyEventHook;
            private static WinEventDelegate onProc = new WinEventDelegate(OnProcMsg);

            public void Register()
            {
                destroyEventHook = SetWinEventHook(
                        WinEvents.EVENT_OBJECT_DESTROY, WinEvents.EVENT_OBJECT_DESTROY,
                        IntPtr.Zero,
                        onProc, 0, 0,
                        WinEventFlags.WINEVENT_OUTOFCONTEXT | WinEventFlags.WINEVENT_SKIPOWNPROCESS
                    );
            }

            public void UnRegister()
            {
                UnhookWinEvent(destroyEventHook);
            }
        }
    }

}
