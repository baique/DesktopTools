using BeanFramework.core.bean;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeanFrameworkTest.bean
{
    [Bean(Name = "测试C")]
    internal class BasicBeanC
    {
        public BasicBeanA BasicBeanA { get; set; }
        public BasicBeanB BasicBeanB { get; set; }

        public BasicBeanC(BasicBeanA a)
        {
            BasicBeanA = a;
        }
        public BasicBeanC(BasicBeanA a, BasicBeanB b)
        {
            this.BasicBeanA = a;
            this.BasicBeanB = b;
        }
    }
}
