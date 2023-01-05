namespace Realm.Domain;

public abstract class Component
{
    public Entity Entity { get; internal set; } = default!;

    public virtual Task Load() => Task.CompletedTask;

    public virtual void Destroy() { }
}
