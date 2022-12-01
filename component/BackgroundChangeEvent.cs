using DesktopTools.component.impl;
using DesktopTools.component.support;
using DesktopTools.util;
using System.Windows.Input;

namespace DesktopTools.component
{
    public class BackgroundChangeEvent : EventTrigger<Key, bool>
    {
        public bool Match(Key key)
        {
            return KeyUtil.CheckKeyDown(SettingUtil.GetSettingOrDefValueIfNotExists(SettingUtil.ChangeBiYingBackgroundKey, "LeftCtrl + LeftAlt + B + N"), key);
        }

        public bool Trigger(Key e)
        {
            SystemBackground.ChangeBackground();
            return true;
        }
    }
}
