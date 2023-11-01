namespace RealmCore.Server.Components;

public class ComponentLifecycle : IComponentLifecycle
{
    public Element Element { get; set; }

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
