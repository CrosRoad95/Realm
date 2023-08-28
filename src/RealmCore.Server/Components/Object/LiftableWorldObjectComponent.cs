using RealmCore.ECS;
using RealmCore.Server.Components.Elements.Abstractions;

namespace RealmCore.Server.Components.Object;

[ComponentUsage(false)]
public class LiftableWorldObjectComponent : InteractionComponent
{
    private readonly object _lock = new();
    public Entity? Owner { get; private set; }
    public Entity[]? AllowedForEntities { get; private set; }

    public event Action<LiftableWorldObjectComponent, Entity>? Lifted;
    public event Action<LiftableWorldObjectComponent, Entity>? Dropped;

    public LiftableWorldObjectComponent() { }

    public LiftableWorldObjectComponent(params Entity[] allowedForEntities)
    {
        AllowedForEntities = allowedForEntities;
    }

    public bool TryLift(Entity entity)
    {
        ThrowIfDisposed();

        if (!IsAllowedToLift(entity))
            return false;

        lock (_lock)
        {
            if (Entity == entity)
                return false;

            if (Owner == null)
            {
                Owner = entity;
                Owner.PreDisposed += HandleOwnerPreDisposed;
                Lifted?.Invoke(this, entity);
                return true;
            }
        }
        return false;
    }

    public bool IsAllowedToLift(Entity entity) => AllowedForEntities == null || AllowedForEntities.Contains(entity);

    private void HandleOwnerPreDisposed(Entity disposedEntity)
    {
        Debug.Assert(Owner != null);
        Owner.PreDisposed -= HandleOwnerPreDisposed;
        TryDrop();
    }

    public bool TryDrop()
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            if (Owner == null)
                return false;

            Owner.Disposed -= HandleOwnerPreDisposed;
            Dropped?.Invoke(this, Owner);
            Owner = null;
        }
        return true;
    }
}
