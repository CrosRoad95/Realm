namespace RealmCore.Server.Components.Object;

[ComponentUsage(false)]
public class LiftableWorldObjectComponent : InteractionComponent
{
    private readonly object _lock = new();
    public Entity? Owner { get; private set; }

    public event Action<LiftableWorldObjectComponent, Entity>? Lifted;
    public event Action<LiftableWorldObjectComponent, Entity>? Dropped;

    public LiftableWorldObjectComponent()
    {

    }

    public bool TryLift(Entity entity)
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            if (Entity == entity)
                return false;

            if (Owner == null)
            {
                Owner = entity;
                Owner.Disposed += HandleDisposed;
                Lifted?.Invoke(this, entity);
                return true;
            }
        }
        return false;
    }

    private void HandleDisposed(Entity disposedEntity)
    {
        TryDrop();
    }

    public bool TryDrop()
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            if (Owner == null)
                return false;

            Owner.Disposed -= HandleDisposed;
            Dropped?.Invoke(this, Owner);
            Owner = null;
        }
        return true;
    }

    public override void Dispose()
    {
        TryDrop();
        base.Dispose();
    }
}
