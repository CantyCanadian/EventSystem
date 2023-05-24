using UnityEngine;
using UnityEngine.Assertions;

namespace Canty.Event
{
    public abstract class HierarchalEventListenerBase<EventBaseType> : MonoBehaviour
            where EventBaseType : Internal.EventBase
    {
        protected HierarchalEventDispatcherBase<EventBaseType> _dispatcher = null;

        protected virtual void Awake()
        {
            _dispatcher = GetComponentInParent<HierarchalEventDispatcherBase<EventBaseType>>();
            Assert.IsNotNull(_dispatcher, "Hierarchal Dispatcher not found in hierarchy.");

            _dispatcher.RegisterEventListener(this);
        }
    }
}