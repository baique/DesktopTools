using System;

namespace BeanFramework.core.bean
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class Import : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true)]
    public class Bean : Attribute
    {
        public string Name { get; set; } = "";
        public int Order { get; set; } = 0;
    }
}
