namespace Realm.Domain.Components.Object;

[ComponentUsage(false)]
public class LiftableWorldObjectComponent : InteractionComponent
{
    private readonly object _ownerLock = new();
    public Entity? Owner { get; private set; }
    public LiftableWorldObjectComponent()
    {

    }

    public bool TryLift(Entity entity)
    {
        ThrowIfDisposed();

        lock(_ownerLock)
        {
            if(Owner == null)
            {
                Owner = entity;
                Owner.Disposed += HandleDestroyed;
                return true;
            }
        }
        return false;
    }

    private void HandleDestroyed(Entity _)
    {
        TryDrop();
    }

    public bool TryDrop()
    {
        ThrowIfDisposed();

        lock (_ownerLock)
        {
            if(Owner == null)
            {
                return false;
            }
            Owner.Disposed -= HandleDestroyed;
            Owner = null;
        }
        return true;
    }
}
