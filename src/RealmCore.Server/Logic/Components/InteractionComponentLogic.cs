using RealmCore.Server.Components.Abstractions;

namespace RealmCore.Server.Logic.Components;

internal sealed class InteractionComponentLogic : ComponentLogic<InteractionComponent>
{
    public InteractionComponentLogic(IElementFactory elementFactory) : base(elementFactory)
    {
    }

    protected override void ComponentAdded(InteractionComponent component)
    {
        if(component.Element is IComponents components)
        {
            if (!components.HasComponent<FocusableComponent>())
                components.AddComponent<FocusableComponent>();
        }
    }
}
