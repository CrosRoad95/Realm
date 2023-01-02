namespace Realm.Domain.Components.Players;

[Serializable]
public class StatisticsCounterComponent : Component
{
    private Player _player = default!;

    [Inject]
    private StatisticsCounterService StatisticsCounterService { get; set; } = default!;

    public float TraveledDistanceInVehicleAsDriver { get; set; }
    public float TraveledDistanceInVehicleAsPassager { get; set; }
    public float TraveledDistanceSwimming { get; set; }
    public float TraveledDistanceByFoot { get; set; }
    public float TraveledDistanceInAir { get; set; }

    public float TotalTraveledDistance => TraveledDistanceInVehicleAsDriver + TraveledDistanceInVehicleAsPassager +
        TraveledDistanceSwimming + TraveledDistanceByFoot + TraveledDistanceInAir;
    public StatisticsCounterComponent()
    {
    }

    public override Task Load()
    {
        var player = Entity.GetRequiredComponent<PlayerElementComponent>().Player;
        player.Disconnected += HandlePlayerDisconnected;
        return Task.CompletedTask;
    }

    private void HandlePlayerDisconnected(Player sender, SlipeServer.Server.Elements.Events.PlayerQuitEventArgs e)
    {
        StatisticsCounterService.SetCounterEnabledFor(_player, false);
    }

    private void HandleStatisticsCollected(Player statisticsOfPlayer, Dictionary<string, float> statistics)
    {
        var player = Entity.GetRequiredComponent<PlayerElementComponent>().Player;
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
