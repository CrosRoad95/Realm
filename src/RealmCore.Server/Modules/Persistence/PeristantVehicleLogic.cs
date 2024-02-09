namespace RealmCore.Server.Modules.Persistence;

internal sealed class PeristantVehicleLogic : VehicleLogic
{
    private readonly IActiveVehicles _activeVehicles;

    public PeristantVehicleLogic(IElementFactory elementFactory, IActiveVehicles activeVehicles) : base(elementFactory)
    {
        _activeVehicles = activeVehicles;
    }

    protected override void HandleLoaded(IVehiclePersistanceFeature persistatnce, RealmVehicle vehicle)
    {
        _activeVehicles.TrySetActive(persistatnce.Id, vehicle);
    }
}
