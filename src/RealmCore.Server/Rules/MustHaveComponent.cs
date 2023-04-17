namespace RealmCore.Server.Rules;

public sealed class MustHaveComponent<TComponent> : IEntityRule
    where TComponent : Component
{
    public bool Check(Entity entity)
    {
        return entity.HasComponent<TComponent>();
    }
}
