namespace Realm.Server.Rules;

public sealed class MustBePlayerOnFootOnlyRule : IEntityRule
{
    public bool Check(Entity entity)
    {
        if (entity.Tag != EntityTag.Player)
            return false;

        return entity.GetRequiredComponent<PlayerElementComponent>().OccupiedVehicle == null;
    }
}
