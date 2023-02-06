namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class CurrentInteractEntityComponent : Component
{
    public Entity CurrentInteractEntity { get; }
    public CurrentInteractEntityComponent(Entity entity)
    {
        CurrentInteractEntity = entity;
    }
}
