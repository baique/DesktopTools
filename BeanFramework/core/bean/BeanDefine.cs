using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BeanFramework.core.bean
{
    public class BeanDefine
    {
        private Context _context;
        public Type Type { get; set; }
        public string? Name { get; private set; }
        public object? Instance { get; set; }
        public Type[] RelyType { get; set; } = new Type[0];
        public bool Loaded { get; set; } = false;
        public bool Created { get; set; } = false;
        public bool Inited { get; set; } = false;
        public bool IsComponentImpl { get; set; } = false;
        public BeanDefine(Context context, Type type)
        {
            _context = context;
            Type = type;
            Name = type.Name;
        }

        /// <summary>
        /// 尝试使用无参构造创建
        /// </summary>
        /// <returns>是否成功</returns>
        public bool TryInitBean()
        {
            try
            {
                Instance = Activator.CreateInstance(Type);

                if (Instance != null)
                {
                    Created = true;
                    SetInstanceName();
                    return true;
                }
            }
            catch { }
            return false;
        }

        private void SetInstanceName()
        {
            var com = Instance as Component;
            if (com != null) Name = string.IsNullOrWhiteSpace(com.Name) ? Name : com.Name;
            var beanDefine = Type.GetCustomAttribute(typeof(Bean), true) as Bean;
            if (beanDefine != null) Name = string.IsNullOrWhiteSpace(beanDefine.Name) ? Name : beanDefine.Name;
        }

        /// <summary>
        /// 尝试适用有参构造创建
        /// </summary>
        /// <returns>是否成功</returns>
        public bool TryConstructor()
        {
            var cs = Type.GetConstructors();

            foreach (var c in cs.OrderByDescending(f => f.GetParameters().Length))
            {
                if (tryCreate(c))
                {
                    SetInstanceName();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 初始化Bean属性
        /// </summary>
        public void LoadBeanAttr()
        {
            var prop = Type.GetProperties();
            foreach (var item in prop)
            {
                if (!item.CanWrite) continue;
                var beanAttr = item.GetCustomAttribute(typeof(Import), true);
                if (beanAttr == null) continue;
                if (item.PropertyType.IsGenericType && item.PropertyType.GetGenericTypeDefinition() != null && item.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)))
                {
                    var t = item.PropertyType.GetGenericArguments()[0];
                    item.SetValue(Instance, _context.GetBeanList(t));
                }
                else
                {
                    var p = _context.GetBean(item.PropertyType);
                    if (p == null) continue;
                    item.SetValue(Instance, p);
                }

            }

            Loaded = true;
        }

        public void Init()
        {
            try
            {
                var com = Instance as Component;
                if (com != null)
                {
                    IsComponentImpl = true;
                    com.Init();
                    return;
                }
            }
            finally
            {
                Inited = true;
            }
        }

        public void Destory()
        {
            var com = Instance as Component;
            if (com != null)
            {
                com.Destroy();
            }
        }

        private bool tryCreate(ConstructorInfo c)
        {
            var d = c.GetParameters();
            var ps = new object[d.Length];
            for (int i = 0; i < d.Length; i++)
            {
                var bean = _context.GetBean(d[i].ParameterType);
                if (bean == null)
                {
                    return false;
                }
                ps[i] = bean;
            }
            try
            {
                Instance = c.Invoke(ps);
                Created = Instance != null;
                return true;
            }
            catch { }

            return false;
        }
    }
}
