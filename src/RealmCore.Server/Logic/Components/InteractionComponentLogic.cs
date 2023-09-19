namespace RealmCore.Server.Logic.Components;

internal sealed class InteractionComponentLogic : ComponentLogic<InteractionComponent>
{
    public InteractionComponentLogic(IEntityEngine entityEngine) : base(entityEngine)
    {
    }

    protected override void ComponentAdded(InteractionComponent component)
    {
        if (!component.Entity.HasComponent<FocusableComponent>())
            component.Entity.AddComponent<FocusableComponent>();
    }
}
