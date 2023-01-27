namespace Realm.Domain.Components.Object;

public class LiftableWorldObjectComponent : Component
{
    public LiftableWorldObjectComponent()
    {

    }

    public override Task LoadAsync()
    {
        Entity.GetRequiredComponent<ElementComponent>().AddFocusable();
        return base.LoadAsync();
    }

    public override void Dispose()
    {
        Entity.GetRequiredComponent<ElementComponent>().RemoveFocusable();
        base.Dispose();
    }
}
