namespace Realm.Server.Logic.Resources;

internal class AFKLogic
{
    private readonly EventScriptingFunctions _eventScriptingFunctions;

    public AFKLogic(AFKService AFKService, EventScriptingFunctions eventScriptingFunctions)
    {

        AFKService.PlayerAFKStarted += HandlePlayerAFKStarted;
        AFKService.PlayerAFKSStopped += HandlePlayerAFKSStopped;
        _eventScriptingFunctions = eventScriptingFunctions;
    }

    private async void HandlePlayerAFKStarted(Player player)
    {
        using var afkStateChangedEvent = new PlayerAFKStateChangedEvent((RPGPlayer)player, true);
        await _eventScriptingFunctions.InvokeEvent(afkStateChangedEvent);
    }

    private async void HandlePlayerAFKSStopped(Player player)
    {
        using var afkStateChangedEvent = new PlayerAFKStateChangedEvent((RPGPlayer)player, false);
        await _eventScriptingFunctions.InvokeEvent(afkStateChangedEvent);
    }
}
