using Canty.Event.Internal;
using UnityEngine;

namespace Canty.Event
{
    public class HierarchalEventDispatcherBase<EventBaseType> : MonoBehaviour
        where EventBaseType : EventBase
    {
        protected EventDispatcherObject<EventBaseType> _dispatcherObject = new EventDispatcherObject<EventBaseType>();

        public void RegisterEventListener(HierarchalEventListenerBase<EventBaseType> listener) => _dispatcherObject.RegisterEventListener(listener);
        public void SendEvent<EventType>(EventType eventObject) where EventType : EventBaseType => _dispatcherObject.SendEvent(eventObject);
        public void SendEventImmediately<EventType>(EventType eventObject) where EventType : EventBaseType => _dispatcherObject.SendEventImmediately(eventObject);
    }
}