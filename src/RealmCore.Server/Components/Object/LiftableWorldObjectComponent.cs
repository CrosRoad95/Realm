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
        if (!IsAllowedToLift(entity))
            return false;

        lock (_lock)
        {
            if (Entity == entity)
                return false;

            if (Owner == null)
            {
                Owner = entity;
                Owner.Disposed += HandleOwnerDisposed;
                Lifted?.Invoke(this, entity);
                return true;
            }
        }
        return false;
    }

    public bool IsAllowedToLift(Entity entity) => AllowedForEntities == null || AllowedForEntities.Contains(entity);

    private void HandleOwnerDisposed(Entity disposedEntity)
    {
        Debug.Assert(Owner != null);
        Owner.Disposed -= HandleOwnerDisposed;
        TryDrop();
    }

    public bool TryDrop()
    {
        lock (_lock)
        {
            if (Owner == null)
                return false;

            Owner.Disposed -= HandleOwnerDisposed;
            Dropped?.Invoke(this, Owner);
            Owner = null;
        }
        return true;
    }
}
