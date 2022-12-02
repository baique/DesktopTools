using BeanFramework.core.bean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeanFrameworkTest.bean
{
    [Bean(Name = "测试A")]
    internal class BasicBeanA
    {
        [Import]
        public BasicBeanB BasicBeanB { get; set; }
    }
}
