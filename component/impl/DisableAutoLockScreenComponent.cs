using DesktopTools.util;
using System;

namespace DesktopTools.component.impl
{
    public class DisableAutoLockScreenComponent
    {
        public static void TriggerUserMouseEvent()
        {
            Win32.mouse_event(Win32.MouseEventFlag.Move, 0, 0, 0, UIntPtr.Zero);
        }

        public static void ToggleSwitch()
        {
            if ("1".Equals(SettingUtil.GetSetting(SettingUtil.EnableDisableLockScreenKey, "1")))
            {
                SettingUtil.SetSetting(SettingUtil.EnableDisableLockScreenKey, "0");
                MainWindow.Notify.ShowBalloonTip(300, "提示", "禁止自动锁屏已关闭", System.Windows.Forms.ToolTipIcon.Info);
            }
            else
            {
                SettingUtil.SetSetting(SettingUtil.EnableDisableLockScreenKey, "1");
                MainWindow.Notify.ShowBalloonTip(300, "提示", "禁止自动锁屏已开启", System.Windows.Forms.ToolTipIcon.Info);
            }
        }
    }
}
