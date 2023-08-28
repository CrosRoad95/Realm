using RealmCore.ECS.Components;
using RealmCore.Server.Components.Elements.Abstractions;

namespace RealmCore.Server.Components.Common;

public class FocusableComponent : Component
{
    protected override void Attach()
    {
        Entity.GetRequiredComponent<ElementComponent>().AddFocusable();
    }

    protected override void Detach()
    {
        Entity.GetRequiredComponent<ElementComponent>().RemoveFocusable();
    }
}
