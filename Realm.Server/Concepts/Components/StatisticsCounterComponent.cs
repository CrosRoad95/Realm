namespace Realm.Server.Concepts.Components;

[Serializable]
public class StatisticsCounterComponent : IElementComponent
{
    private RPGPlayer _player = default!;
    private StatisticsCounterService _statisticsCounterService = default!;

    [ScriptMember("name")]
    public string Name => "StatisticsCounter";

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

    public StatisticsCounterComponent(SerializationInfo info, StreamingContext context)
    {
        TraveledDistanceInVehicleAsDriver = (float)info.GetDecimal(nameof(TraveledDistanceInVehicleAsDriver));
        TraveledDistanceInVehicleAsPassager = (float)info.GetDecimal(nameof(TraveledDistanceInVehicleAsPassager));
        TraveledDistanceSwimming = (float)info.GetDecimal(nameof(TraveledDistanceSwimming));
        TraveledDistanceByFoot = (float)info.GetDecimal(nameof(TraveledDistanceByFoot));
        TraveledDistanceInAir = (float)info.GetDecimal(nameof(TraveledDistanceInAir));
    }

    [NoScriptAccess]
    public void SetOwner(Element element)
    {
        if (_player != null)
            throw new Exception("Component already attached to element");
        if (element is not RPGPlayer rpgPlayer)
            throw new Exception("Not supported element type, expected: RPGPlayer");
        _player = rpgPlayer;
        _player.LoggedOut += LoggedOut;
        _statisticsCounterService = rpgPlayer.MtaServer.GetRequiredService<StatisticsCounterService>();
        _statisticsCounterService.SetCounterEnabledFor(_player, true);
        _statisticsCounterService.StatisticsCollected += StatisticsCounterService_StatisticsCollected;
    }

    private void StatisticsCounterService_StatisticsCollected(Player player, Dictionary<string, float> statistics)
    {
        if (player != _player)
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

    private void LoggedOut(RPGPlayer player, string accountId)
    {
        _player.LoggedOut -= LoggedOut;
        _statisticsCounterService.SetCounterEnabledFor(_player, false);
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(TraveledDistanceInVehicleAsDriver), TraveledDistanceInVehicleAsDriver);
        info.AddValue(nameof(TraveledDistanceInVehicleAsPassager), TraveledDistanceInVehicleAsPassager);
        info.AddValue(nameof(TraveledDistanceSwimming), TraveledDistanceSwimming);
        info.AddValue(nameof(TraveledDistanceByFoot), TraveledDistanceByFoot);
        info.AddValue(nameof(TraveledDistanceInAir), TraveledDistanceInAir);
    }
}
