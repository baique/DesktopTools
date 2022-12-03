using DesktopTools.component.impl;
using DesktopTools.util;

namespace DesktopToolsTest
{
    [TestFixture, Order(0)]
    public class DisableAutoLockScreenTimerTest
    {
        [Test]
        public void Test()
        {
            //取得默认状态
            var defSetting = SettingUtil.GetSetting(SettingUtil.EnableDisableLockScreenKey);
            try
            {
                //设置开启
                SettingUtil.SetSetting(SettingUtil.EnableDisableLockScreenKey, "1");
                Assert.AreEqual("1", SettingUtil.GetSetting(SettingUtil.EnableDisableLockScreenKey), "background setting fail");

                DisableAutoLockScreenComponent.TriggerUserMouseEvent();

                //设置关闭
                SettingUtil.SetSetting(SettingUtil.EnableDisableLockScreenKey, "0");
                Assert.AreEqual("0", SettingUtil.GetSetting(SettingUtil.EnableDisableLockScreenKey), "background setting fail");

            }
            finally
            {
                //设置默认
                SettingUtil.SetSetting(SettingUtil.EnableDisableLockScreenKey, defSetting);
            }
        }
    }
}
