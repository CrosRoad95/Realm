namespace RealmCore.Server.Components;

public class OwnerDisposableComponent : Component
{
    public Entity OwningEntity { get; }

    public OwnerDisposableComponent(Entity owningEntity)
    {
        OwningEntity = owningEntity;
        OwningEntity.Disposed += HandleDisposed;
    }

    private void HandleDisposed(Entity entity)
    {
        Entity.Dispose();
    }

    public override void Dispose()
    {
        OwningEntity.Disposed -= HandleDisposed;
    }
}
