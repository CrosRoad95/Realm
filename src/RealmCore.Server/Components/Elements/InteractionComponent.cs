namespace RealmCore.Server.Components.Elements;

public abstract class InteractionComponent : Component
{
    public virtual float MaxInteractionDistance { get; } = 1.3f;

    public InteractionComponent()
    {

    }

    protected override void Load()
    {
        Entity.GetRequiredComponent<ElementComponent>().AddFocusable();
    }

    protected override void Detached()
    {
        Entity.GetRequiredComponent<ElementComponent>().RemoveFocusable();
    }
}
