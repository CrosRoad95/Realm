namespace RealmCore.Server.Components.Common;

public class OwnerDisposableComponent : Component
{
    public Entity OwningEntity { get; }

    public OwnerDisposableComponent(Entity owningEntity)
    {
        OwningEntity = owningEntity;
        OwningEntity.PreDisposed += HandlePreDisposed;
    }

    protected override void Detach()
    {
        OwningEntity.PreDisposed -= HandlePreDisposed;
    }

    private void HandlePreDisposed(Entity entity)
    {
        Dispose();
    }
}
