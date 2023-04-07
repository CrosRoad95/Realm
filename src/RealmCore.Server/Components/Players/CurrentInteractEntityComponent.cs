namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class CurrentInteractEntityComponent : Component
{
    public Entity? CurrentInteractEntity { get; private set; }
    private object _lock = new();

    public CurrentInteractEntityComponent(Entity entity)
    {
        CurrentInteractEntity = entity;

        CurrentInteractEntity.Disposed += HandleDisposed;
    }

    private void HandleDisposed(Entity etntiy)
    {
        lock (_lock)
        {
            if (CurrentInteractEntity != null)
                CurrentInteractEntity.Disposed -= HandleDisposed;
            CurrentInteractEntity = null!;
        }

        Entity.DestroyComponent(this);
    }

    public override void Dispose()
    {
        lock (_lock)
        {
            if (CurrentInteractEntity != null)
                CurrentInteractEntity.Disposed -= HandleDisposed;
            CurrentInteractEntity = null!;
        }

        base.Dispose();
    }
}
