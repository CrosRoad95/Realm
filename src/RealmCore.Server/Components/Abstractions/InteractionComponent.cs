namespace RealmCore.Server.Components.Abstractions;

public abstract class InteractionComponent : ComponentLifecycle
{
    public virtual float MaxInteractionDistance { get; } = 1.3f;

    public InteractionComponent()
    {

    }
}
