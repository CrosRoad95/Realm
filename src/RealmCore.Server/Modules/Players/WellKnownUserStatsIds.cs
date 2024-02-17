namespace RealmCore.Server.Modules.Players;

public enum WellKnownUserStatsIds : int
{
    Unknown,
    TraveledDistanceInVehicleAsDriver,
    TraveledDistanceInVehicleAsPassenger,
    TraveledDistanceSwimming,
    TraveledDistanceByFoot,
    TraveledDistanceInAir,

    UserStatsIdOffset = 1000,
}