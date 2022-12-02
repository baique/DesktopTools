using BeanFramework.core.bean;
using DesktopTools.component.support;
using DesktopTools.util;
using System.Windows.Input;
using System.Xml.Linq;

namespace DesktopTools.component.key_event
{
    [Bean(Name = "快捷键强制绑定窗体")]
    public class ForceRegisterWindowEvent : EventTrigger<Key, bool>
    {
        public bool Match(Key key)
        {
            return KeyUtil.CheckKeyDown(SettingUtil.GetSettingOrDefValueIfNotExists(SettingUtil.ForceWindowBindOrChangeKey, "LeftCtrl + LeftAlt"), key)
                && (key >= Key.NumPad0 && key <= Key.NumPad9 || key >= Key.D0 && key <= Key.D9);
        }

        public bool Trigger(Key e)
        {
            ToggleWindow.RegisterKeyWindow(e, Win32.GetForegroundWindow());
            return true;
        }

        public int Order()
        {
            return -100;
        }
    }
}
