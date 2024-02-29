namespace RealmCore.Server.Modules.Persistence;

internal sealed class PeristantVehicleLogic : VehicleLifecycle
{
    private readonly IVehiclesInUse _activeVehicles;

    public PeristantVehicleLogic(IElementFactory elementFactory, IVehiclesInUse activeVehicles) : base(elementFactory)
    {
        _activeVehicles = activeVehicles;
    }

    protected override void HandleLoaded(IVehiclePersistenceFeature persistatnce, RealmVehicle vehicle)
    {
        _activeVehicles.TrySetActive(persistatnce.Id, vehicle);
    }
}
