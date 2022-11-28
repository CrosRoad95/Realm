namespace Realm.Server.Services;

internal class RPGPlayerService
{
    private readonly EventScriptingFunctions _eventScriptingFunctions;

    public RPGPlayerService(EventScriptingFunctions eventScriptingFunctions)
    {
        _eventScriptingFunctions = eventScriptingFunctions;
    }

    public void RelayEvent<TEvent>(TEvent @event)
        where TEvent : INamedLuaEvent, IDisposable
    {
        Task.Run(async () =>
        {
            using (@event)
            {
                await _eventScriptingFunctions.InvokeEvent(@event);
            }
        });
    }
}
