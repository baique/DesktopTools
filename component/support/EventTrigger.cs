namespace DesktopTools.component.support
{
    public interface EventTrigger<IN, OUT>
    {
        public bool Match(IN? param) { return true; }
        public OUT? Trigger(IN? param) { return default; }
        public int Order() { return 0; }
    }
}
