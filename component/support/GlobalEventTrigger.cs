using DesktopTools.util;
using System;
using System.Collections.Generic;

namespace DesktopTools.component.support
{
    public class GlobalEventTrigger<IN, OUT> : ResourceHook
    {
        private List<EventTrigger<IN, OUT>> _events = new List<EventTrigger<IN, OUT>>();

        public void AddEvent(EventTrigger<IN, OUT> e)
        {
            lock (this)
            {
                if (_events.Contains(e))
                {
                    return;
                }
                _events.Add(e);
            }
        }
        public List<EventTrigger<IN, OUT>> GetEvents()
        {
            return _events;
        }
        public OUT? Handler(IN? param)
        {
            foreach (var item in _events)
            {
                try
                {
                    if (item.Match(param))
                    {
                        return item.Trigger(param);
                    }
                }
                catch
                {
                }
            }
            return default;
        }

        public void HandlerMulti(IN? param)
        {
            foreach (var item in _events)
            {
                try
                {
                    if (item.Match(param))
                    {
                        item.Trigger(param);
                    }
                }
                catch
                {
                }
            }
        }


        public void Register()
        {
            var supportTypes = InstanceUtil.GetSupport(typeof(EventTrigger<IN, OUT>));
            foreach (var item in supportTypes)
            {
                var eventObj = Activator.CreateInstance(item) as EventTrigger<IN, OUT>;
                if (eventObj != null)
                {
                    var newEvent = eventObj;
                    if (newEvent is ResourceHook)
                    {
                        ((ResourceHook)newEvent).Register();
                    }
                    AddEvent(newEvent);
                }
            }
            _events.Sort((a, b) => a.Order() - b.Order());
        }

        public void UnRegister()
        {
            foreach (var item in GetEvents())
            {
                if (item is ResourceHook)
                {
                    try
                    {
                        ((ResourceHook)item).UnRegister();
                    }
                    catch { }
                }
            }
        }
    }
}
