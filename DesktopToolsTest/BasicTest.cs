using BeanFramework.core;
using BeanFramework.core.bean;
using DesktopTools;
using DesktopTools.component;
using DesktopTools.component.key_event;

namespace DesktopToolsTest
{
    [Order(int.MinValue),TestFixture, Apartment(ApartmentState.STA)]
    public class BeanLoadedTest
    {
        [TestCase(typeof(AutoChangeBackground))]
        [TestCase(typeof(DisableAutoLockScreenTimer))]
        [TestCase(typeof(GlobalSystemKeyPressEvent))]
        [TestCase(typeof(GoodbyeTimer))]
        [TestCase(typeof(ToggleFullScreenEvent))]
        [TestCase(typeof(ToggleWindow))]
        [TestCase(typeof(ViewEvent))]
        [TestCase(typeof(ViewEvent.BindViewRenameEvent))]
        [TestCase(typeof(ViewEvent.BindViewDestroyEvent))]
        [TestCase(typeof(BackgroundChangeEvent), false)]
        [TestCase(typeof(DisableAutoLockScreenEvent), false)]
        [TestCase(typeof(ForceRegisterWindowEvent), false)]
        [TestCase(typeof(UnRegisterWindowEvent), false)]
        public void Test(Type type, bool isImplComponent = true)
        {
            BeanDefine? bean = SetUpTest.context.GetBeanDefine(type);
            Assert.IsNotNull(bean, "not created");
            Assert.IsTrue(bean.Created, "not set created status");
            Assert.IsTrue(bean.Loaded, "not loaded");
            if (isImplComponent)
            {
                Assert.IsTrue(bean.IsComponentImpl, "type not match");
            }
            Assert.IsTrue(bean.Inited, "not inited");
        }
    }
}