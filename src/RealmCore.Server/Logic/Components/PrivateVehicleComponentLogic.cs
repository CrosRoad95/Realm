namespace RealmCore.Server.Logic.Components;

internal sealed class PrivateVehicleComponentLogic : ComponentLogic<PrivateVehicleComponent>
{
    private readonly IActiveVehicles _activeVehicles;

    public PrivateVehicleComponentLogic(IElementFactory elementFactory, IActiveVehicles activeVehicles) : base(elementFactory)
    {
        _activeVehicles = activeVehicles;
    }

    protected override void ComponentDetached(PrivateVehicleComponent privateVehicleComponent)
    {
        _activeVehicles.TrySetInactive(privateVehicleComponent.Id);
    }
}

