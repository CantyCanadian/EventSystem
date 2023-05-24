namespace Canty.Event
{
    public class ExampleGlobalEventDispatcher : GlobalEventDispatcherBase<Internal.ExampleEventBase> { }

    public class ExampleHierarchalEventDispatcher : HierarchalEventDispatcherBase<Internal.ExampleEventBase> { }
}