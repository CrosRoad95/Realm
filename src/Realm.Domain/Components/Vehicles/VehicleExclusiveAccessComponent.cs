namespace Realm.Domain.Components.Vehicles;

public class VehicleExclusiveAccessComponent : VehicleAccessControllerComponent
{
    private readonly Entity _entity;

    public VehicleExclusiveAccessComponent(Entity targetPlayerEntity)
    {
        _entity = targetPlayerEntity;
        _entity.Destroyed += HandleTargetEntityDestroyed;
    }

    private void HandleTargetEntityDestroyed(Entity obj)
    {
        Entity.DestroyComponent(this);
        _entity.Destroyed -= HandleTargetEntityDestroyed;
    }

    protected override void Load()
    {
        base.Load();
        Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle.CanEnter = CanEnter;
    }

    protected override bool CanEnter(Ped ped, Vehicle vehicle)
    {
        if(_entity.TryGetComponent(out PlayerElementComponent playerElementComponent))
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
