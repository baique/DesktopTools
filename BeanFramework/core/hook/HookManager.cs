using System.Collections.Generic;

namespace BeanFramework.core.hook
{
    public class HookManager
    {
        public static Dictionary<string, object> hookPart = new Dictionary<string, object>();

        public static HookPart<IN, OUT> Register<IN, OUT>(HookPart<IN, OUT> part)
        {
            if (hookPart.ContainsKey(part.EventName))
            {
                return (HookPart<IN, OUT>)hookPart[part.EventName];
            }
            hookPart[part.EventName] = part;
            return part;
        }
    }
}
