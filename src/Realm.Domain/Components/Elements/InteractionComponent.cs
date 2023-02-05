namespace Realm.Domain.Components.Object;

public class InteractionComponent : Component
{
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
