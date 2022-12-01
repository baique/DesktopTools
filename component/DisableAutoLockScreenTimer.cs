using DesktopTools.component.impl;
using DesktopTools.component.support;
using DesktopTools.util;
using System.Timers;

namespace DesktopTools.component
{
    public class DisableAutoLockScreenTimer : EventTrigger<MainWindow, bool>
    {
        private Timer? timer;
        #region 禁止自动锁屏
        private void RegisterDisableAutoLockScreen()
        {
            timer = new Timer();
            timer.Interval = 5000;
            timer.AutoReset = true;
            timer.Elapsed += (a, e) =>
            {
                if (!"1".Equals(SettingUtil.GetSetting(SettingUtil.EnableDisableLockScreenKey, "1"))) return;
                DisableAutoLockScreenComponent.TriggerUserMouseEvent();
            };
            timer.Start();
        }
        #endregion
        public bool Trigger(MainWindow? e)
        {
            RegisterDisableAutoLockScreen();
            return true;
        }
    }
}
