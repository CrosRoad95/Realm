namespace RealmCore.Server.Rules;

public sealed class MustBePlayerRule : IEntityRule
{
    public bool Check(Entity entity)
    {
        if (!entity.HasComponent<PlayerTagComponent>())
            return false;

        return true;
    }
}
