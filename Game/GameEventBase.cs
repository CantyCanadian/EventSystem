using System.Runtime.CompilerServices;
using System.IO;

namespace Canty.Event
{
    public abstract class GameEventBase<T> : Internal.GameEventBase
        where T : GameEventBase<T>
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        public static T GetEvent([CallerFilePath] string origin = "")
        {
            return _poolObject.GetEvent<T>(Path.GetFileNameWithoutExtension(origin));
        }
#else
        public static T GetEvent()
        {
            return _poolObject.GetEvent<T>();
        }
#endif
    }
}

namespace Canty.Event.Internal
{
    public abstract class GameEventBase : EventBase
    {
        protected static EventPoolObject _poolObject = new EventPoolObject();

        protected override void OnIsProcessedChanged(bool flag)
        {
            if (!flag)
                _poolObject.ReleaseEvent(this);
        }
    }
}