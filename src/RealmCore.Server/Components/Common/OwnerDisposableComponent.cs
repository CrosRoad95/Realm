using RealmCore.ECS;
using RealmCore.ECS.Components;

namespace RealmCore.Server.Components.Common;

public class OwnerDisposableComponent : Component
{
    public Entity OwningEntity { get; }

    public OwnerDisposableComponent(Entity owningEntity)
    {
        OwningEntity = owningEntity;
        OwningEntity.PreDisposed += HandlePreDisposed;
    }

    private void HandlePreDisposed(Entity entity)
    {
        Entity.Dispose();
    }

    public override void Dispose()
    {
        OwningEntity.Disposed -= HandlePreDisposed;
    }
}
