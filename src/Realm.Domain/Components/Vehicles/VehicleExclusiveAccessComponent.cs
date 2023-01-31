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

    public override void Load()
    {
        if(Entity.Tag != Entity.VehicleTag)
            throw new NotSupportedException("This component only works on vehicles.");

        if (Entity.Components.OfType<VehicleAccessControllerComponent>().Where(x => x != this).Any())
            throw new InvalidOperationException("Vehicle already have vehicle access controller component");

        Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle.CanEnter = CanEnter;
    }

    public bool CanEnter(Ped ped, Vehicle vehicle)
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
