namespace RealmCore.Server.Components.Elements.Abstractions;

public abstract class InteractionComponent : Component
{
    public virtual float MaxInteractionDistance { get; } = 1.3f;

    public InteractionComponent()
    {

    }

    protected override void Attach()
    {
        Entity.GetRequiredComponent<ElementComponent>().AddFocusable();
    }

    protected override void Detach()
    {
        Entity.GetRequiredComponent<ElementComponent>().RemoveFocusable();
    }
}
