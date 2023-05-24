using Canty.Event;
using Canty.Event.Internal;
using UnityEngine;

namespace Canty
{
    public class RandomGlobalEventExampleClass : MonoBehaviour
    {
        [EventListener]
        private void OnNumberSetEventReceived(ExampleNumberSetEvent numberSetEvent)
        {
            // This function will be called when an ExampleNumberSetEvent is sent through the dispatcher.
        }

        [EventListener]
        private void OnEventReceived(ExampleEventBase eventBase)
        { 
            // This function will be called when any event inheriting from ExampleEventBase is sent through the dispatcher.
        }

        private void Start()
        {
            // You wish to register your listener in Awake and send your initial events in Start, as to prevent any timing issues.
            // Here's how you send events.
            ExampleNumberSetEvent numberSetEvent = ExampleNumberSetEvent.GetEvent();
            numberSetEvent.Reset(8);
            ExampleGlobalEventDispatcher.SendEvent(numberSetEvent);

            // You can also send an event immediately. Usually, events are queued up and are send sequencially (one finishes, another start).
            // However, there are cases where you need an event to be sent the moment you call SendEvent, bypassing the queue.

            // Note, never reuse an event object. Call GetEvent() each time an event is sent. Think of sending the event as deleting it.
            numberSetEvent = ExampleNumberSetEvent.GetEvent();
            numberSetEvent.Reset(44);
            ExampleGlobalEventDispatcher.SendEventImmediately(numberSetEvent);
        }

        private void Awake()
        {
            // You wish to register your listener in Awake and send your initial events in Start, as to prevent any timing issues.
            ExampleGlobalEventDispatcher.RegisterListener(this);
        }
    }
}