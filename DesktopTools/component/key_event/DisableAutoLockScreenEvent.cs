using BeanFramework.core.bean;
using DesktopTools.component.impl;
using DesktopTools.component.support;
using DesktopTools.util;
using System.Windows.Input;

namespace DesktopTools.component.key_event
{
    [Bean(Name = "快捷键切换是否禁用自动锁屏")]
    public class DisableAutoLockScreenEvent : EventTrigger<Key, bool>
    {
        public bool Match(Key key)
        {
            return KeyUtil.CheckKeyDown(SettingUtil.GetSettingOrDefValueIfNotExists(SettingUtil.ChangeEnableDisableLockScreenKey, "LeftCtrl + LeftAlt + Space"), key);
        }

        public bool Trigger(Key e)
        {
            DisableAutoLockScreenComponent.ToggleSwitch();
            return true;
        }
    }
}
