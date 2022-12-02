using BeanFramework.core.bean;
using DesktopTools.component.impl;
using DesktopTools.component.support;
using DesktopTools.util;
using System.Windows.Input;

namespace DesktopTools.component.key_event
{
    [Bean(Name = "快捷键紧急避险模式")]
    public class UpdateWindowToggleEvent : EventTrigger<Key, bool>
    {
        public bool Match(Key key)
        {
            return KeyUtil.CheckKeyDown(SettingUtil.GetSettingOrDefValueIfNotExists(SettingUtil.ErrorModeKey, "LeftCtrl + LeftShift + Space"), key);
        }

        public bool Trigger(Key e)
        {
            GlobalSystemKeyPressEvent.GlobalKeybordEventStatus = false;
            GoodbyeModeComponent.GlobalEnable = false;
            WindowUpdate loading = new WindowUpdate();
            loading.ShowDialog();
            GoodbyeModeComponent.GlobalEnable = true;
            GlobalSystemKeyPressEvent.GlobalKeybordEventStatus = true;
            return true;
        }
        public int Order()
        {
            return int.MinValue;
        }
    }
}
