namespace RealmCore.ECS.Common;

public class ComponentLifecycle : IComponentLifecycle
{
    public Entity Entity { get; set; }

    public event Action<IComponentLifecycle>? Attached;
    public event Action<IComponentLifecycle>? Detached;

    public virtual void Attach()
    {
        Attached?.Invoke(this);
    }

    public virtual void Detach()
    {
        Detached?.Invoke(this);
    }

    public virtual void Dispose() { }
}
