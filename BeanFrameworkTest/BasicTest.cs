using BeanFramework.core;
using BeanFramework.core.bean;
using BeanFrameworkTest.bean;
using static BeanFrameworkTest.bean.BeanTest;

namespace BeanFrameworkTest
{
    public class BasicTest
    {
        /// <summary>
        /// basic test
        /// </summary>
        [Test]
        public void Test()
        {
            Context ctx = new Context();
            ctx.Start(typeof(BasicTest));
            BasicBeanA a = ctx.GetBean(typeof(BasicBeanA)) as BasicBeanA;
            Assert.IsNotNull(a);
            Assert.IsNotNull(a.BasicBeanB);

            BasicBeanB b = ctx.GetBean(typeof(BasicBeanB)) as BasicBeanB;
            Assert.IsNotNull(b);
            Assert.IsNotNull(b.BasicBeanA);

            BasicBeanC c = ctx.GetBean(typeof(BasicBeanC)) as BasicBeanC;
            Assert.IsNotNull(c);
            Assert.IsNotNull(c.BasicBeanA);
            Assert.IsNotNull(c.BasicBeanB);

            var beans = ctx.GetBeanList(typeof(CustomBeanDefine)) as List<CustomBeanDefine>;
            Assert.IsNotNull(beans);
            Assert.AreEqual(5, beans.Count);

            ctx.Shutdown();
        }
    }
}
