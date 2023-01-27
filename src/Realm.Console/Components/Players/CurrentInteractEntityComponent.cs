namespace Realm.Console.Components.Players;

internal class CurrentInteractEntityComponent : Component
{
    public Entity CurrentInteractEntity { get; }
    public CurrentInteractEntityComponent(Entity entity)
    {
        CurrentInteractEntity = entity;
    }
}
