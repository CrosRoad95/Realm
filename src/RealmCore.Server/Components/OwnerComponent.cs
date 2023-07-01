namespace RealmCore.Server.Components;

public class OwnerComponent : Component
{
    public Entity OwningEntity { get; }

    public OwnerComponent(Entity owningEntity)
    {
        OwningEntity = owningEntity;
        OwningEntity.Disposed += HandleDisposed;
    }

    private void HandleDisposed(Entity entity)
    {
        Entity.DestroyComponent(this);
    }

    public override void Dispose()
    {
        OwningEntity.Disposed -= HandleDisposed;
    }
}
