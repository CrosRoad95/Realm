using Realm.Domain.Sessions;
using Realm.Module.Scripting.Extensions;
using Realm.Module.Scripting.Functions;

namespace Realm.Server.Logic;

internal class RPGFractionLogic
{
    private readonly EventScriptingFunctions _eventFunctions;
    private readonly IRPGElementsFactory _rpgElementsFactory;

    public RPGFractionLogic(EventScriptingFunctions eventFunctions, IRPGElementsFactory rpgElementsFactory)
    {
        _eventFunctions = eventFunctions;
        _rpgElementsFactory = rpgElementsFactory;
        _rpgElementsFactory.FractionCreated += HandleFractionCreated;
    }

    private void HandleFractionCreated(RPGFraction fraction)
    {
        fraction.SessionStarted += HandleSessionStarted;
        fraction.SessionStopped += HandleSessionStopped;
    }

    private async void HandleSessionStarted(Player player, FractionSession session)
    {
        using var playerSessionStarted = new PlayerSessionStartedEvent((RPGPlayer)player, session);
        await _eventFunctions.InvokeEvent(playerSessionStarted);
    }

    private async void HandleSessionStopped(Player player, FractionSession session)
    {
        using var playerSessionStarted = new PlayerSessionStoppedEvent((RPGPlayer)player, session);
        await _eventFunctions.InvokeEvent(playerSessionStarted);
    }
}
