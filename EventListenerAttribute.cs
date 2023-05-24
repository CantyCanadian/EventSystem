using System;

namespace Canty.Event
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EventListenerAttribute : Attribute
    {
        public EventListenerAttribute() { }
    }
}