using BeanFramework.core.bean;

namespace BeanFrameworkTest.bean
{
    internal class BeanTest
    {
        [Bean]
        public class CustomBeanDefine
        {

        }
        public class Bean1 : CustomBeanDefine { }
        public class Bean2 : CustomBeanDefine { }
        public class Bean3 : CustomBeanDefine { }
    }
}
