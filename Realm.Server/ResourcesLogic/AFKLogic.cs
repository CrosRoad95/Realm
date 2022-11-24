using Realm.Resources.AFK;
using Realm.Server.Elements;

namespace Realm.Server.ResourcesLogic;

internal class AFKLogic
{
    private readonly EventScriptingFunctions _eventScriptingFunctions;

    public AFKLogic(AFKService AFKService, EventScriptingFunctions eventScriptingFunctions)
    {

        AFKService.PlayerAFKStarted += AFKService_PlayerAFKStarted;
        AFKService.PlayerAFKSStopped += AFKService_PlayerAFKSStopped;
        _eventScriptingFunctions = eventScriptingFunctions;
    }

    private async void AFKService_PlayerAFKStarted(Player player)
    {
        using var afkStateChangedEvent = new PlayerAFKStateChangedEvent((RPGPlayer)player, true);
        await _eventScriptingFunctions.InvokeEvent(afkStateChangedEvent);
    }

    private async void AFKService_PlayerAFKSStopped(Player player)
    {
        using var afkStateChangedEvent = new PlayerAFKStateChangedEvent((RPGPlayer)player, false);
        await _eventScriptingFunctions.InvokeEvent(afkStateChangedEvent);
    }
}
