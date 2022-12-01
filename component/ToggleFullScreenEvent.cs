using DesktopTools.component.support;
using DesktopTools.util;
using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using static DesktopTools.util.Win32;

namespace DesktopTools.component
{
    public class ToggleFullScreenEvent : EventTrigger<MainWindow, bool>, ResourceHook
    {
        private static APPBARDATA abd;
        private HwndSource source;

        public static IntPtr ForegoundChangeProcMsg(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == abd.uCallbackMessage && wParam.ToInt32() == (int)ABNotify.ABN_FULLSCREENAPP)
            {
                if ((int)lParam == 1) SettingUtil.HasFullScreen = true;
                else SettingUtil.HasFullScreen = false;
            }
            return IntPtr.Zero;
        }

        public void Register()
        {
        }

        public bool Trigger(MainWindow? mainWindow)
        {
            abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = AppUtil.GetHwnd(mainWindow);
            abd.uCallbackMessage = RegisterWindowMessage("APPBARMSG_CSDN_HELPER");
            SHAppBarMessage((int)ABMsg.ABM_NEW, ref abd);
            source = HwndSource.FromHwnd(abd.hWnd);
            source.AddHook(new HwndSourceHook(ForegoundChangeProcMsg));
            return true;
        }

        public void UnRegister()
        {
            SHAppBarMessage((int)ABMsg.ABM_REMOVE, ref abd);
            source.Dispose();
        }
    }
}
