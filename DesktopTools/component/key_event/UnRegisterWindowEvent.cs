using BeanFramework.core.bean;
using DesktopTools.component.support;
using DesktopTools.util;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Linq;

namespace DesktopTools.component.key_event
{
    [Bean(Name = "快捷键接触绑定窗体")]
    public class UnRegisterWindowEvent : EventTrigger<Key, bool>
    {
        public bool Match(Key key)
        {
            return KeyUtil.CheckKeyDown(
                SettingUtil.GetSettingOrDefValueIfNotExists(SettingUtil.UnWindowBindOrChangeKey, "LeftCtrl + LeftAlt + Back"), key);
        }

        public bool Trigger(Key e)
        {
            if (MessageBox.Show("当前窗体将被移除全部快捷访问,是否继续？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification) == DialogResult.Yes) ToggleWindow.RemoveKeyWindow();
            return true;
        }

        public int Order()
        {
            return -101;
        }
    }
}
