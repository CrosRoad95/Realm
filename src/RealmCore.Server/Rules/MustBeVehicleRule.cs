namespace RealmCore.Server.Rules;

public sealed class MustBeVehicleRule : IEntityRule
{
    public bool Check(Entity entity)
    {
        if (!entity.HasComponent<VehicleTagComponent>())
            return false;

        return true;
    }
}
