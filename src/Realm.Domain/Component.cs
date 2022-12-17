namespace Realm.Domain;

public abstract class Component
{
    [ScriptMember("entity")]
    public Entity Entity { get; set; } = default!;

    public virtual Task Load() => Task.CompletedTask;

}
