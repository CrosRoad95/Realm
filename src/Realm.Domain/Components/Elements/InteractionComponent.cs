namespace Realm.Domain.Components.Elements;

public class InteractionComponent : Component
{
    public virtual float MaxInteractionDistance { get; } = 1.3f;

    public InteractionComponent()
    {

    }

    protected override void Load()
    {
        Entity.GetRequiredComponent<ElementComponent>().AddFocusable();
    }

    public override void Dispose()
    {
        Entity.GetRequiredComponent<ElementComponent>().RemoveFocusable();
        base.Dispose();
    }
}
