using Realm.Interfaces.Server;
using Realm.Scripting.Functions;

namespace Realm.Scripting.Extensions;

public static class EventFunctionsExtensions
{
    public static async Task InvokeEvent<TEvent>(this EventScriptingFunctions eventFunctions, TEvent @event)
        where TEvent : INamedLuaEvent, IDisposable
    {
        await eventFunctions.InvokeEvent(TEvent.EventName, @event);
    }
}
