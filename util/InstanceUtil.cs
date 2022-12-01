using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace DesktopTools.util
{
    public class InstanceUtil
    {
        public static List<Type> GetSupport(Type clazz)
        {
            List<Type> list = new List<Type>();
            Assembly? ass = Assembly.GetAssembly(clazz);
            Type[]? types = ass?.GetTypes();
            if (types == null) return new List<Type>(0);
            foreach (Type? item in types)
            {
                if (item.IsInterface) continue;
                Type[] ins = item.GetInterfaces();
                if(item == typeof(component.GlobalSystemKeyPressEvent))
                {
                    Trace.Write(item);
                }
                foreach (Type ty in ins)
                {
                    if (ty == clazz)
                    {
                        list.Add(item);
                        break;
                    }
                }
            }
            return list;
        }

        public static object? GetInstance(Type type, params Type[] types)
        {
            var t = type.MakeGenericType(types);

            return Activator.CreateInstance(t);
        }

        private static bool IsGenericTypeMatch(Type ty, Type[] genericClazz)
        {
            var gta = ty.GenericTypeArguments;
            for (int i = 0; i < genericClazz.Length; i++)
            {
                if(genericClazz[i] != gta[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
