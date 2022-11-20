using DesktopTools.util;
using System;
using System.Windows.Input;
using static DesktopTools.component.GlobalKeyboardEvent;

namespace DesktopTools.component
{
    public class DisableAutoLockScreen : Event
    {
        public static void TriggerUserMouseEvent()
        {
            Win32.mouse_event(Win32.MouseEventFlag.Move, 0, 0, 0, UIntPtr.Zero);
        }

        public bool Handler(KeyEventArgs e)
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
            return true;
        }

        public string? Key()
        {
            return SettingUtil.GetSettingOrDefValueIfNotExists(SettingUtil.ChangeEnableDisableLockScreenKey, "LeftCtrl + LeftAlt + Space");
        }
    }
}
