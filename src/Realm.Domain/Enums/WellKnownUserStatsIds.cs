namespace Realm.Domain.Enums;

public enum WellKnownUserStatsIds : int
{
    Unknown,
    TraveledDistanceInVehicleAsDriver,
    TraveledDistanceInVehicleAsPassager,
    TraveledDistanceSwimming,
    TraveledDistanceByFoot,
    TraveledDistanceInAir,
    
    UserStatsIdOffset = 1000,
}