namespace Realm.Domain.Rules;

public sealed class MustBePlayerOnFootOnlyRule : IEntityRule
{
    public bool Check(Entity entity)
    {
        if (entity.Tag != Entity.EntityTag.Player)
            return false;

        return entity.GetRequiredComponent<PlayerElementComponent>().OccupiedVehicle == null;
    }
}
