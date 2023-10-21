namespace RealmCore.Server.Logic.Components;

internal sealed class PrivateVehicleComponentLogic : ComponentLogic<PrivateVehicleComponent>
{
    private readonly IActiveVehicles _activeVehicles;

    public PrivateVehicleComponentLogic(IEntityEngine entityEngine, IActiveVehicles activeVehicles) : base(entityEngine)
    {
        _activeVehicles = activeVehicles;
    }

    protected override void ComponentDetached(PrivateVehicleComponent privateVehicleComponent)
    {
        _activeVehicles.TrySetInactive(privateVehicleComponent.Id);
    }
}

