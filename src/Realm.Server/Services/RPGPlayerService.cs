using Realm.Module.Scripting.Extensions;
using Realm.Module.Scripting.Functions;

namespace Realm.Server.Services;

internal class RPGPlayerService
{
    private readonly EventScriptingFunctions _eventScriptingFunctions;
    private readonly AccountsInUseService _accountsInUseService;

    public RPGPlayerService(EventScriptingFunctions eventScriptingFunctions, AccountsInUseService accountsInUseService)
    {
        _eventScriptingFunctions = eventScriptingFunctions;
        _accountsInUseService = accountsInUseService;
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
