using Realm.Domain.Components.Elements;

namespace Realm.Domain.Components.Players;

[Serializable]
public class StatisticsCounterComponent : Component
{
    private Player _player = default!;
    private StatisticsCounterService _statisticsCounterService = default!;

    [ScriptMember("traveledDistanceInVehicleAsDriver")]
    public float TraveledDistanceInVehicleAsDriver { get; set; }
    [ScriptMember("traveledDistanceInVehicleAsPassager")]
    public float TraveledDistanceInVehicleAsPassager { get; set; }
    [ScriptMember("traveledDistanceSwimming")]
    public float TraveledDistanceSwimming { get; set; }
    [ScriptMember("traveledDistanceByFoot")]
    public float TraveledDistanceByFoot { get; set; }
    [ScriptMember("traveledDistanceInAir")]
    public float TraveledDistanceInAir { get; set; }

    [ScriptMember("totalTraveledDistance")]
    public float TotalTraveledDistance => TraveledDistanceInVehicleAsDriver + TraveledDistanceInVehicleAsPassager +
        TraveledDistanceSwimming + TraveledDistanceByFoot + TraveledDistanceInAir;
    public StatisticsCounterComponent()
    {
    }

    public override Task Load()
    {
        var player = Entity.InternalGetRequiredComponent<PlayerElementCompoent>().Player;
        player.Disconnected += HandlePlayerDisconnected;
        return Task.CompletedTask;
    }

    private void HandlePlayerDisconnected(Player sender, SlipeServer.Server.Elements.Events.PlayerQuitEventArgs e)
    {
        _statisticsCounterService.SetCounterEnabledFor(_player, false);
    }

    private void HandleStatisticsCollected(Player statisticsOfPlayer, Dictionary<string, float> statistics)
    {
        var player = Entity.InternalGetRequiredComponent<PlayerElementCompoent>().Player;
        if (player != statisticsOfPlayer)
            return;
#pragma warning disable IDE0018 // Inline variable declaration
        float value;
#pragma warning restore IDE0018 // Inline variable declaration

        if (statistics.TryGetValue("traveledDistanceInVehicleAsDriver", out value))
            TraveledDistanceInVehicleAsDriver += value;
        if (statistics.TryGetValue("traveledDistanceInVehicleAsPassager", out value))
            TraveledDistanceInVehicleAsPassager += value;
        if (statistics.TryGetValue("traveledDistanceSwimming", out value))
            TraveledDistanceSwimming += value;
        if (statistics.TryGetValue("traveledDistanceByFoot", out value))
            TraveledDistanceByFoot += value;
        if (statistics.TryGetValue("traveledDistanceInAir", out value))
            TraveledDistanceInAir += value;
    }
}
