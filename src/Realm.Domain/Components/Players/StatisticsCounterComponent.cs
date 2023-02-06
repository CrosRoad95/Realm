namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class StatisticsCounterComponent : Component
{
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
    
    internal StatisticsCounterComponent(Statistics statistics)
    {
        TraveledDistanceInVehicleAsDriver = statistics.TraveledDistanceInVehicleAsDriver;
        TraveledDistanceInVehicleAsPassager = statistics.TraveledDistanceInVehicleAsPassager;
        TraveledDistanceSwimming = statistics.TraveledDistanceSwimming;
        TraveledDistanceByFoot = statistics.TraveledDistanceByFoot;
        TraveledDistanceInAir = statistics.TraveledDistanceInAir;
    }

    protected override void Load()
    {
        var player = Entity.GetRequiredComponent<PlayerElementComponent>().Player;
        StatisticsCounterService.StatisticsCollected += HandleStatisticsCollected;
        StatisticsCounterService.SetCounterEnabledFor(player, true);
    }

    public override void Dispose()
    {
        base.Dispose();
        var player = Entity.GetRequiredComponent<PlayerElementComponent>().Player;
        StatisticsCounterService.StatisticsCollected -= HandleStatisticsCollected;
        StatisticsCounterService.SetCounterEnabledFor(player, false);
    }

    private void HandleStatisticsCollected(Player statisticsOfPlayer, Dictionary<string, float> statistics)
    {
        Entity.GetRequiredComponent<PlayerElementComponent>().Compare(statisticsOfPlayer);
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
