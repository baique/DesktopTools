using BeanFramework.core.bean;
using BeanFramework.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BeanFramework.core
{
    public class Context
    {
        private Dictionary<Type, BeanDefine> ContextBeanDefines = new Dictionary<Type, BeanDefine>();

        public void Start(Type scanPackage)
        {
            HashSet<Type> components = new HashSet<Type>();
            foreach (Type t in InstanceUtil.GetSupport(typeof(Component), typeof(Context), scanPackage))
                components.Add(t);
            foreach (Type t in InstanceUtil.GetSupportAttr(typeof(Bean), typeof(Context), scanPackage))
                components.Add(t);


            // load all component 
            foreach (var component in components)
            {
                if (component.IsAbstract || component.IsInterface) continue;
                if (ContextBeanDefines.ContainsKey(component))
                {
                    continue;
                }
                ContextBeanDefines.Add(component, new BeanDefine(this, component));
            }
            List<BeanDefine> UnLoadedBeanDefines = new List<BeanDefine>();
            List<BeanDefine> UnCreatedBeanDefines = new List<BeanDefine>();
            foreach (var item in ContextBeanDefines)
            {
                if (item.Value.TryInitBean()) UnLoadedBeanDefines.Add(item.Value);
                else UnCreatedBeanDefines.Add(item.Value);
            }
            foreach (var item in UnCreatedBeanDefines)
            {
                if (item.TryConstructor())
                {
                    UnLoadedBeanDefines.Add(item);
                }
                else
                {
                    throw new Exception(string.Format("bean load fail:{0}", item.Type.Name));
                }
            }
            foreach (var item in UnLoadedBeanDefines)
            {
                item.LoadBeanAttr();
                item.Init();
                Trace.WriteLine(string.Format("bean {0} loaded", item.Name));
            }

        }

        public object? GetBean(Type type)
        {
            if (ContextBeanDefines.ContainsKey(type))
            {
                var define = ContextBeanDefines[type];
                if (define.Created) return define.Instance;
            }
            return null;
        }

        public object GetBeanList(Type type)
        {
            List<object> d = new List<object>();
            foreach (var item in ContextBeanDefines)
            {
                if (InstanceUtil.IsSupport(item.Key, type))
                {
                    if (item.Value.Instance != null) d.Add(item.Value.Instance);
                }
            }

            Type listType = typeof(List<>);
#pragma warning disable CS8600
            object list = Activator.CreateInstance(listType.MakeGenericType(type));
#pragma warning disable CS8602
            var addMethod = list.GetType().GetMethod("Add");
            d.OrderBy(f =>
            {
                var com = f as Component;
                if (com != null) return com.Order();
                return 0;
            }).ToList()
            .ForEach(f =>
            {
                addMethod.Invoke(list, new object[] { f });
            });
            return list;
        }

        public void Shutdown()
        {

        }

        public void Register(Type type, object instance)
        {
            var define = new BeanDefine(this, type)
            {
                Instance = instance,
                Loaded = true,
                Created = true,
            };
            ContextBeanDefines[type] = define;
            define.Init();
        }
    }
}
