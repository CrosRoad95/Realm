namespace RealmCore.Server.Logic.Components;

internal sealed class PrivateVehicleComponentLogic : VehicleLogic
{
    private readonly IActiveVehicles _activeVehicles;

    public PrivateVehicleComponentLogic(IElementFactory elementFactory, IActiveVehicles activeVehicles) : base(elementFactory)
    {
        _activeVehicles = activeVehicles;
    }

    protected override void HandleLoaded(IVehiclePersistanceService persistatnce, RealmVehicle vehicle)
    {
        _activeVehicles.TrySetInactive(persistatnce.Id);
    }
}

