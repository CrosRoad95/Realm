namespace Realm.Server.Components.Vehicles;

public class VehicleExclusiveAccessComponent : VehicleAccessControllerComponent
{
    private readonly Entity _entity;

    public VehicleExclusiveAccessComponent(Entity targetPlayerEntity)
    {
        _entity = targetPlayerEntity;
        _entity.Disposed += HandleTargetEntityDestroyed;
    }

    private void HandleTargetEntityDestroyed(Entity obj)
    {
        Entity.DestroyComponent(this);
        _entity.Disposed -= HandleTargetEntityDestroyed;
    }

    protected override void Load()
    {
        base.Load();
        Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle.CanEnter = CanEnter;
    }

    protected override bool CanEnter(Ped ped, Vehicle vehicle)
    {
        if (_entity.TryGetComponent(out PlayerElementComponent playerElementComponent))
        {
            return playerElementComponent.Player == ped;
        }
        return false;
    }

    public override void Dispose()
    {
        Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle.CanEnter = null;
    }
}
