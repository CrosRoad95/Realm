namespace RealmCore.Server.Components.Vehicles.Access;

public class VehicleExclusiveAccessComponent : VehicleAccessControllerComponent
{
    private readonly Entity _targetEntity;

    public VehicleExclusiveAccessComponent(Entity targetEntity)
    {
        _targetEntity = targetEntity;
        _targetEntity.Disposed += HandleTargetEntityDestroyed;
    }

    private void HandleTargetEntityDestroyed(Entity obj)
    {
        Entity.DestroyComponent(this);
        _targetEntity.Disposed -= HandleTargetEntityDestroyed;
    }

    protected override bool CanEnter(Entity pedEntity, Entity vehicleEntity)
    {
        return _targetEntity == pedEntity;
    }
}
