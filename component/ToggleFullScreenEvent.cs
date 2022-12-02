using BeanFramework.core.bean;
using DesktopTools.util;
using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using static DesktopTools.util.Win32;

namespace DesktopTools.component
{
    [Bean(Name = "全屏时主动隐藏")]
    public class ToggleFullScreenEvent : Component
    {
        private static APPBARDATA abd;
        private HwndSource? source;

        public static IntPtr ForegoundChangeProcMsg(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == abd.uCallbackMessage && wParam.ToInt32() == (int)ABNotify.ABN_FULLSCREENAPP)
            {
                if ((int)lParam == 1) SettingUtil.HasFullScreen = true;
                else SettingUtil.HasFullScreen = false;
            }
            return IntPtr.Zero;
        }

        public void Init()
        {
            abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = AppUtil.GetHwnd(App.Current.MainWindow);
            abd.uCallbackMessage = RegisterWindowMessage("APPBARMSG_CSDN_HELPER");
            SHAppBarMessage((int)ABMsg.ABM_NEW, ref abd);
            source = HwndSource.FromHwnd(abd.hWnd);
            source.AddHook(new HwndSourceHook(ForegoundChangeProcMsg));
        }


        public void Destroy()
        {
            SHAppBarMessage((int)ABMsg.ABM_REMOVE, ref abd);
            if (source != null) source.Dispose();
        }
    }
}
