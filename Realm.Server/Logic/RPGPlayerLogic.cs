using System;

namespace Realm.Server.Logic;

internal class RPGPlayerLogic
{
    private readonly EventScriptingFunctions _eventFunctions;

    public RPGPlayerLogic(MtaServer mtaServer, EventScriptingFunctions eventFunctions)
    {
        mtaServer.PlayerJoined += MtaServer_PlayerJoined;
        _eventFunctions = eventFunctions;
    }

    private void MtaServer_PlayerJoined(Player player)
    {
        var rpgPlayer = (RPGPlayer)player;
        rpgPlayer.LoggedIn += RpgPlayer_LoggedIn;
        rpgPlayer.LoggedOut += RpgPlayer_LoggedOut;
        rpgPlayer.Spawned += RpgPlayer_Spawned;
    }

    private async void RpgPlayer_Spawned(RPGPlayer player, RPGSpawn spawn)
    {
        using var playerSpawnedEvent = new PlayerSpawnedEvent(player, spawn);
        await _eventFunctions.InvokeEvent(playerSpawnedEvent);
    }

    private async void RpgPlayer_LoggedOut(RPGPlayer player, string id)
    {
        using var playerLoggedOutEvent = new PlayerLoggedOutEvent(player);
        await _eventFunctions.InvokeEvent(playerLoggedOutEvent);
    }

    private async void RpgPlayer_LoggedIn(RPGPlayer player, PlayerAccount account)
    {
        using var playerLoggedInEvent = new PlayerLoggedInEvent(player, account);
        await _eventFunctions.InvokeEvent(playerLoggedInEvent);
    }
}
