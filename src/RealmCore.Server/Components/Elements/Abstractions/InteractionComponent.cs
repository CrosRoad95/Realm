namespace RealmCore.Server.Components.Elements.Abstractions;

public abstract class InteractionComponent : Component
{
    public virtual float MaxInteractionDistance { get; } = 1.3f;

    public InteractionComponent()
    {

    }
}
