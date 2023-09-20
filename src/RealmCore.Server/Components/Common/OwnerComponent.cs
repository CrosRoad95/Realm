namespace RealmCore.Server.Components.Common;

public class OwnerComponent : Component
{
    public Entity OwningEntity { get; }

    public OwnerComponent(Entity owningEntity)
    {
        OwningEntity = owningEntity;
    }

    protected override void Attach()
    {
        if (OwningEntity == Entity)
            throw new ArgumentException(nameof(OwningEntity));

        OwningEntity.PreDisposed += HandlePreDisposed;
        base.Attach();
    }

    private void HandlePreDisposed(Entity entity)
    {
        Entity.DestroyComponent(this);
    }

    public override void Dispose()
    {
        OwningEntity.PreDisposed -= HandlePreDisposed;
    }
}
