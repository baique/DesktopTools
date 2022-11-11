using DesktopTools.util;
using DesktopTools.views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DesktopTools.component.GlobalKeyboardEvent;

namespace DesktopTools.component
{
    public class DisableAutoLockScreen : Event

    {
        public static void TriggerUserMouseEvent()
        {
            Win32.mouse_event(Win32.MouseEventFlag.Move, 0, 0, 0, UIntPtr.Zero);
        }

        public void Handler(KeyEventArgs e)
        {
            if ("1".Equals(Setting.GetSetting(Setting.EnableDisableLockScreenKey, "1")))
            {
                Setting.SetSetting(Setting.EnableDisableLockScreenKey, "0");
                MainWindow.Notify.ShowBalloonTip(300, "提示", "禁止自动锁屏已关闭", ToolTipIcon.Info);
            }
            else
            {
                Setting.SetSetting(Setting.EnableDisableLockScreenKey, "1");
                MainWindow.Notify.ShowBalloonTip(300, "提示", "禁止自动锁屏已开启", ToolTipIcon.Info);
            }
        }

        public string Key()
        {
            return Setting.GetSetting(Setting.ChangeEnableDisableLockScreenKey, "LeftCtrl + LeftAlt + Space");
        }
    }
}
