namespace Canty.Event.Internal
{
    public abstract class EventBase
    {
        public string Origin { get; set; } = string.Empty;

        public virtual void Copy(EventBase eventObject)
        {
            Origin = eventObject.Origin;
        }

        public abstract string GetDebugData();

        protected virtual void OnIsProcessedChanged(bool flag) { }
    }
}