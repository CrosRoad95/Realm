namespace RealmCore.Server.Modules.Elements.Interactions;

public abstract class Interaction : IDisposable
{
    public virtual float MaxDistance { get; } = 1.3f;

    public virtual void Dispose() { }
}
