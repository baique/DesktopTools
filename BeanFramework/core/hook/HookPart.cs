namespace BeanFramework.core.hook
{
    public class HookPart<IN, OUT>
    {
        public string EventName { get; set; }
        public string? EventDescription { get; set; }
        private Func<IN, OUT> EventHandler;
        private List<EventHook<IN, OUT>> EventMount = new List<EventHook<IN, OUT>>();

        public HookPart(string name)
        {
            this.EventName = name;
            this.EventHandler = (i) =>
            {
                foreach (var item in EventMount)
                {
                    if (item.Match(i))
                    {
                        return item.Trigger(i);
                    }
                }
                return default;
            };
        }
        public HookPart(string name, Func<IN, OUT> eventHandler)
        {
            this.EventName = name;
            EventHandler = eventHandler;
        }

        public OUT Trigger(IN i)
        {
            return EventHandler.Invoke(i);
        }

        public void Register(EventHook<IN, OUT> children)
        {
            EventMount.Add(children);
        }
    }
}
