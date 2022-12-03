using BeanFramework.core.bean;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeanFrameworkTest.bean
{
    [Bean(Name = "测试B")]
    internal class BasicBeanB
    {
        public BasicBeanA BasicBeanA { get; set; }
        public BasicBeanB(BasicBeanA a)
        {
            this.BasicBeanA = a;
        }
    }
}
