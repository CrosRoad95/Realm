namespace Realm.Domain.Rules;

public sealed class MustBePlayerRule : IEntityRule
{
    public bool Check(Entity entity)
    {
        if (entity.Tag != EntityTag.Player)
            return false;

        return true;
    }
}
