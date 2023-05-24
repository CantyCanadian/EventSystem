using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Canty.Event.Internal
{
    public class EventDispatcherObject<EventBaseType>
        where EventBaseType : EventBase
    {
        private struct EventListener
        {
            public MethodInfo Method { get; }
            public object Target { get; }
            public bool IsMono { get; }

            public EventListener(MethodInfo method, object target, bool isMono)
            {
                Method = method;
                Target = target;
                IsMono = isMono;
            }
        }

        private Dictionary<Type, List<EventListener>> _listeners = new Dictionary<Type, List<EventListener>>();
        private Queue<EventBaseType> _eventQueue = new Queue<EventBaseType>();

        /// <summary>
        /// Since we can assure that events on the other doesn't change as they are sent, we're caching them here to prevent any mid-sending changes.
        /// </summary>
        private Dictionary<Type, EventBaseType> _eventCache = new Dictionary<Type, EventBaseType>();

        private bool _isProcessing = false;

        private Coroutine _refCoroutine = null;

        public void RegisterEventListener(object listener)
        {
            List<(MethodInfo Method, EventListenerAttribute Attribute, ParameterInfo[] Params)> methods = listener.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(method => (Method: method, Attribute: method.GetCustomAttribute<EventListenerAttribute>(), Params: method.GetParameters()))
                .Where(data => data.Attribute != null &&
                            (data.Params.Length == 1 && data.Params[0].ParameterType.IsSubclassOf(typeof(EventBaseType))) ||
                            (data.Params.Length == 2 && data.Params[0].ParameterType.IsSubclassOf(typeof(EventBaseType)) && data.Params[1].ParameterType == typeof(Coroutine))).ToList();

            int methodCount = 0;
            foreach ((MethodInfo Method, EventListenerAttribute Attribute, ParameterInfo[] Params) in methods)
            {
                if (Params[0].ParameterType == typeof(EventBaseType))
                {
                    if (!_listeners.TryGetValue(typeof(EventBaseType), out List<EventListener> values))
                        _listeners.Add(typeof(EventBaseType), values = new List<EventListener>());

                    values.Add(new EventListener(Method, listener, listener.GetType().IsSubclassOf(typeof(MonoBehaviour))));

                    methodCount++;
                }
                else if (Params[0].ParameterType.IsSubclassOf(typeof(EventBaseType)))
                {
                    if (!_listeners.TryGetValue(Params[0].ParameterType, out List<EventListener> values))
                        _listeners.Add(Params[0].ParameterType, values = new List<EventListener>());

                    values.Add(new EventListener(Method, listener, listener.GetType().IsSubclassOf(typeof(MonoBehaviour))));

                    methodCount++;
                }
            }

            Debug.Log($"Registered [{methodCount}] methods.");
        }

        public void SendEvent(EventBaseType eventObject)
        {
            Type eventType = eventObject.GetType();
            if (_eventQueue.Contains(eventObject))
            {
                Debug.LogError($"You're reusing an event object before it can get processed. Either find a way to merge the events or create new objects for each.");
                return;
            }

            if (!_eventCache.ContainsKey(eventType))
                _eventCache.Add(eventType, (EventBaseType)Activator.CreateInstance(eventType));

            _eventQueue.Enqueue(eventObject);

            if (!_isProcessing)
            {
                _isProcessing = true;

                while (_eventQueue.Count > 0)
                {
                    EventBaseType currentEvent = _eventQueue.Dequeue();

                    SendEventInternal(currentEvent);
                }

                _isProcessing = false;
            }
        }

        public void SendEventImmediately(EventBaseType eventObject)
        {
            Type eventType = eventObject.GetType();
            if (_eventQueue.Contains(eventObject))
            {
                Debug.LogError($"You're reusing an event object before it can get processed. Either find a way to merge the events or create new objects for each.");
                return;
            }

            if (!_eventCache.ContainsKey(eventType))
                _eventCache.Add(eventType, (EventBaseType)Activator.CreateInstance(eventType));

            SendEventInternal(eventObject);
        }

        private void SendEventInternal(EventBaseType eventObject)
        {
            Type eventType = eventObject.GetType();

            // We're caching the latest of each events to prevent issues where an event object could be changed mid-way.
            _eventCache[eventType].Copy(eventObject);
            eventObject = _eventCache[eventType];

            string debugData = eventObject.GetDebugData();
            if (!string.IsNullOrEmpty(debugData))
                Debug.Log(debugData);

            if (_listeners.TryGetValue(typeof(EventBaseType), out List<EventListener> genericMethods))
            {
                for (int i = 0; i < genericMethods.Count; ++i)
                {
                    if (genericMethods[i].Target != null)
                    {
                        Invoke(genericMethods[i].Method, genericMethods[i].Target, eventObject);
                    }
                    else
                    {
                        genericMethods.RemoveAt(i);
                        --i;
                    }
                }
            }

            if (_listeners.TryGetValue(eventType, out List<EventListener> specificMethods))
            {
                object convertedEvent = Convert.ChangeType(eventObject, eventType);

                for (int i = 0; i < specificMethods.Count; ++i)
                {
                    if (specificMethods[i].Target != null)
                    {
                        Invoke(specificMethods[i].Method, specificMethods[i].Target, convertedEvent);
                    }
                    else
                    {
                        specificMethods.RemoveAt(i);
                        --i;
                    }
                }
            }
        }

        private void Invoke(MethodInfo method, object target, params object[] parameters)
        {
            if (method.ReturnType == typeof(IEnumerator))
            {
                MonoBehaviour mono = target as MonoBehaviour;
                if (method.GetParameters().Length == 2 && method.GetParameters()[1].ParameterType == typeof(Coroutine))
                    _refCoroutine = mono.StartCoroutine(CoroutineWrapper(mono, method, parameters));
                else
                    mono.StartCoroutine((IEnumerator)method.Invoke(mono, parameters));
            }
            else
            {
                method.Invoke(target, parameters);
            }
        }

        // Very much a hack, but let me explain.
        // Goal :   I want to give a Coroutine object to the called Coroutine since the StartCoroutine call is done under the hood. Thus, unless this succeed,
        //          we cannot track the completion of a Coroutine from within the Listener class.
        // Hypothesis : Calling StartCoroutine causes two things. One, it calls the function until it hits a yield command. Two, it then stores where that yield happen and offloads the call to the Coroutine system.
        //              Ergo, if we force a yield return null right before our actual Coroutine call, it forces the Coroutine system to take over, giving us a fully functional Coroutine object.
        // Issue/Solution : You cannot pass an object by reference in an IEnumerator, which is why said object is passed via a class field.
        private IEnumerator CoroutineWrapper(MonoBehaviour target, MethodInfo method, object[] parameters)
        {
            yield return null;
            yield return target.StartCoroutine((IEnumerator)method.Invoke(target, parameters.Append(_refCoroutine).ToArray()));
            _refCoroutine = null;
        }
    }
}