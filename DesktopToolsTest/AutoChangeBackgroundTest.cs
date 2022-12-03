using DesktopTools.component.impl;
using DesktopTools.util;

namespace DesktopToolsTest
{
    [TestFixture, Order(0)]
    public class AutoChangeBackgroundTest
    {
        [Test]
        public void Test()
        {
            var defaultTime = SystemBackground.getLastChangeBackgroundTime();
            //取得默认状态
            var defSetting = SettingUtil.GetSetting(SettingUtil.ChangeBiYingBackgroundKey);
            try
            {
                //设置开启
                SettingUtil.SetSetting(SettingUtil.ChangeBiYingBackgroundKey, "1");
                Assert.AreEqual("1", SettingUtil.GetSetting(SettingUtil.ChangeBiYingBackgroundKey), "background setting fail");

                SystemBackground.ChangeBackground();
                var lastChangeTime = SystemBackground.getLastChangeBackgroundTime();
                Assert.IsTrue(lastChangeTime != defaultTime, "last change fail");

                var lastChange = DateTime.Now.Subtract(lastChangeTime);
                Assert.IsTrue(lastChange.Seconds < 5, "background change fail");

                //设置关闭
                SettingUtil.SetSetting(SettingUtil.ChangeBiYingBackgroundKey, "0");
                Assert.AreEqual("0", SettingUtil.GetSetting(SettingUtil.ChangeBiYingBackgroundKey), "background setting fail");

                SystemBackground.ChangeBackground();
                var lastChangeTime1 = SystemBackground.getLastChangeBackgroundTime();
                //本次应该未发生壁纸变化
                Assert.IsTrue(lastChangeTime == lastChangeTime1, "close after last change fail");
            }
            finally
            {
                //设置默认
                SettingUtil.SetSetting(SettingUtil.ChangeBiYingBackgroundKey, defSetting);
            }
        }
    }
}
