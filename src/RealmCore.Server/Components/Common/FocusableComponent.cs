namespace RealmCore.Server.Components.Common;

public class FocusableComponent : Component
{
    protected override void Load()
    {
        Entity.GetRequiredComponent<ElementComponent>().AddFocusable();
    }

    protected override void Detached()
    {
        Entity.GetRequiredComponent<ElementComponent>().RemoveFocusable();
    }
}
