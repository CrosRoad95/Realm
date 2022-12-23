namespace Realm.Domain;

public abstract class Component
{
    public Entity Entity { get; set; } = default!;

    public virtual Task Load() => Task.CompletedTask;

    public virtual void Destroy() { }
}
