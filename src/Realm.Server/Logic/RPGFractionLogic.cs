using Realm.Domain.Sessions;

namespace Realm.Server.Logic;

internal class RPGFractionLogic
{
    private readonly EventScriptingFunctions _eventFunctions;
    private readonly IRPGElementsFactory _rpgElementsFactory;

    public RPGFractionLogic(EventScriptingFunctions eventFunctions, IRPGElementsFactory rpgElementsFactory)
    {
        _eventFunctions = eventFunctions;
        _rpgElementsFactory = rpgElementsFactory;
        _rpgElementsFactory.FractionCreated += RPGElementsFactory_FractionCreated;
    }

    private void RPGElementsFactory_FractionCreated(RPGFraction fraction)
    {
        fraction.SessionStarted += Fraction_SessionStarted;
        fraction.SessionStopped += Fraction_SessionStopped;
    }

    private async void Fraction_SessionStarted(Player player, FractionSession session)
    {
        using var playerSessionStarted = new PlayerSessionStartedEvent((RPGPlayer)player, session);
        await _eventFunctions.InvokeEvent(playerSessionStarted);
    }

    private async void Fraction_SessionStopped(Player player, FractionSession session)
    {
        using var playerSessionStarted = new PlayerSessionStoppedEvent((RPGPlayer)player, session);
        await _eventFunctions.InvokeEvent(playerSessionStarted);
    }
}
