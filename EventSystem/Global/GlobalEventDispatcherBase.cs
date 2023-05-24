using Canty.Event.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Canty.Event
{
    [DefaultExecutionOrder(-1)]
    public abstract class GlobalEventDispatcherBase<EventBaseType> : GlobalEventDispatcherBase
        where EventBaseType : EventBase
    {
        public static void RegisterListener(object listener) => GetDispatcher().RegisterEventListenerInternal(listener);
        public static void SendEvent<EventType>(EventType eventObject) where EventType : EventBaseType => GetDispatcher().SendEventInternal(eventObject);
        public static void SendEventImmediately<EventType>(EventType eventObject) where EventType : EventBaseType => GetDispatcher().SendEventImmediatelyInternal(eventObject);

        private static GlobalEventDispatcherBase<EventBaseType> GetDispatcher()
        {
            bool success = _dispatchers.TryGetValue(typeof(EventBaseType), out GlobalEventDispatcherBase dispatcher);
            Assert.IsTrue(success, $"No dispatcher of the right type using event type [{typeof(EventBaseType)}] to register listener to.");

            GlobalEventDispatcherBase<EventBaseType> eventDispatcher = dispatcher as GlobalEventDispatcherBase<EventBaseType>;
            Assert.IsNotNull(eventDispatcher, $"No dispatcher of the right type using event type [{typeof(EventBaseType)}] to register listener to.");

            return eventDispatcher;
        }

        protected EventDispatcherObject<EventBaseType> _dispatcherObject = new EventDispatcherObject<EventBaseType>();

        public void RegisterEventListenerInternal(object listener) => _dispatcherObject.RegisterEventListener(listener);

        public void SendEventInternal<EventType>(EventType eventObject) where EventType : EventBaseType => _dispatcherObject.SendEvent(eventObject);
        public void SendEventImmediatelyInternal<EventType>(EventType eventObject) where EventType : EventBaseType => _dispatcherObject.SendEventImmediately(eventObject);

        private void Awake()
        {
            if (!_dispatchers.ContainsKey(typeof(EventBaseType)))
                _dispatchers.Add(typeof(EventBaseType), this);
        }
    }

    public abstract class GlobalEventDispatcherBase : MonoBehaviour
    {
        protected static Dictionary<Type, GlobalEventDispatcherBase> _dispatchers = new Dictionary<Type, GlobalEventDispatcherBase>();
    }
}