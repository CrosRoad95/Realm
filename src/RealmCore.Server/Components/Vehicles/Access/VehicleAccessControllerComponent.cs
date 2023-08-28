using RealmCore.ECS;
using RealmCore.ECS.Components;

namespace RealmCore.Server.Components.Vehicles.Access;

[ComponentUsage(true)]
public abstract class VehicleAccessControllerComponent : Component
{
    protected abstract bool CanEnter(Entity pedEntity, Entity vehicleEntity);
    internal bool InternalCanEnter(Entity pedEntity, Entity vehicleEntity) => CanEnter(pedEntity, vehicleEntity);
}
