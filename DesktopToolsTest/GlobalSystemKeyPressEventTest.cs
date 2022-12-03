using DesktopTools.component;
using DesktopTools.component.impl;
using DesktopTools.component.key_event;
using DesktopTools.util;

namespace DesktopToolsTest
{
    [TestFixture, Order(0)]
    public class GlobalSystemKeyPressEventTest
    {
        [Test]
        public void Test()
        {
            GlobalSystemKeyPressEvent e = SetUpTest.context.GetBean(typeof(GlobalSystemKeyPressEvent)) as GlobalSystemKeyPressEvent;
            //功能包含检查
            Assert.IsTrue(e.events.Contains(SetUpTest.context.GetBean(typeof(BackgroundChangeEvent))));
            Assert.IsTrue(e.events.Contains(SetUpTest.context.GetBean(typeof(DisableAutoLockScreenEvent))));
            Assert.IsTrue(e.events.Contains(SetUpTest.context.GetBean(typeof(ForceRegisterWindowEvent))));
            Assert.IsTrue(e.events.Contains(SetUpTest.context.GetBean(typeof(UnRegisterWindowEvent))));
        }
    }
}
