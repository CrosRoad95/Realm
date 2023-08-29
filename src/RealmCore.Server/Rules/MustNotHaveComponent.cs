namespace RealmCore.Server.Rules;

public sealed class MustNotHaveComponent<TComponent> : IEntityRule
    where TComponent : Component
{
    public bool Check(Entity entity)
    {
        return !entity.HasComponent<TComponent>();
    }
}
