using System;
using System.Collections.Generic;
using System.Reflection;

namespace BeanFramework.util
{
    public class InstanceUtil
    {
        public static List<Type> GetSupport(Type clazz, params Type[] scan)
        {
            List<Type> list = new List<Type>();
            foreach (Type t in scan)
            {
                list.AddRange(getSupport(clazz, t));
            }
            return list;
        }
        public static List<Type> GetSupportAttr(Type clazz, params Type[] scan)
        {
            List<Type> list = new List<Type>();
            foreach (Type t in scan)
            {
                list.AddRange(getSupport(clazz, t, true));
            }
            return list;
        }

        public static List<Type> getSupport(Type clazz, Type scan, bool isAttr = false)
        {
            List<Type> list = new List<Type>();
            Assembly? ass = Assembly.GetAssembly(scan);
            Type[]? types = ass?.GetTypes();
            if (types == null) return new List<Type>(0);
            foreach (Type? item in types)
            {
                if (IsSupport(item, clazz, isAttr))
                {
                    list.Add(item);
                }
            }
            return list;
        }

        public static bool IsSupport(Type source, Type target, bool isAttr = false)
        {
            if (isAttr)
            {
                if (source.GetCustomAttribute(target, true) != null) return true;
            }
            else if (target.IsAssignableFrom(source))
            {
                return true;
            }
            //类本身并非泛型，或其泛型不符合要求，取得实现接口
            var implInterfaces = source.GetInterfaces();
            foreach (var item in implInterfaces)
            {
                if (IsSupport(item, target, isAttr))
                {
                    return true;
                }
            }
            if (source.BaseType != null)
            {
                return IsSupport(source.BaseType, target, isAttr);
            }
            return false;
        }
    }
}
