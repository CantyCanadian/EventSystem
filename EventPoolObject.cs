using System;
using System.Collections.Generic;

namespace Canty.Event.Internal
{
    public class EventPoolObject
    {
        private static Dictionary<Type, Stack<EventBase>> _eventCache = new Dictionary<Type, Stack<EventBase>>();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        public T GetEvent<T>(string origin)
#else
        public T GetEvent<T>()
#endif
            where T : EventBase
        {
            Type type = typeof(T);
            if (!_eventCache.TryGetValue(type, out Stack<EventBase> cache))
                _eventCache.Add(type, cache = new Stack<EventBase>());

            T result;
            if (cache.Count > 0)
                result = cache.Pop() as T;
            else
                result = Activator.CreateInstance(typeof(T)) as T;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            result.Origin = origin;
#else
            result.Origin = "";
#endif
            return result;
        }

        public void ReleaseEvent(EventBase e)
        {
            Type type = e.GetType();
            if (!_eventCache.TryGetValue(type, out Stack<EventBase> cache))
                _eventCache.Add(type, cache = new Stack<EventBase>());

            cache.Push(e);
        }
    }
}