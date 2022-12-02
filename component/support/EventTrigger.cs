using BeanFramework.core.bean;

namespace DesktopTools.component.support
{
    [Bean]
    public interface EventTrigger<IN, OUT>
    {
        public bool Match(IN? param) { return true; }
        public OUT? Trigger(IN? param) { return default; }
    }
}
