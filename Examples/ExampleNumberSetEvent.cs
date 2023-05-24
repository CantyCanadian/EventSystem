//------------------------------------------------------------------------------
//
// AUTO-GENERATED BY EVENTEDITOR
// DO NOT EDIT
//
//------------------------------------------------------------------------------

using Canty.Event.Internal;


namespace Canty.Event
{
    
    
    public class ExampleNumberSetEvent : ExampleEventBase<ExampleNumberSetEvent>
    {
        
        public int Number { get; private set; }
        
        public void Reset(int number)
        {
            Number = number;
        }
        
        public override void Copy(EventBase eventObject)
        {
            base.Copy(eventObject);
            if (eventObject != null && eventObject is ExampleNumberSetEvent exampleNumberSetEvent)
            {
                Number = exampleNumberSetEvent.Number;
            }
        }
        
        public override string GetDebugData()
        {
            return $"[{nameof(ExampleNumberSetEvent)}] sent by [{Origin}] : Setting a number.";
        }
    }
}