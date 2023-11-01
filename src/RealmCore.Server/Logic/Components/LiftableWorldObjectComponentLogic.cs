namespace RealmCore.Server.Logic.Components;

internal sealed class LiftableWorldObjectComponentLogic : ComponentLogic<LiftableWorldObjectComponent>
{
    public LiftableWorldObjectComponentLogic(IElementFactory elementFactory) : base(elementFactory)
    {
    }

    protected override void ComponentAdded(LiftableWorldObjectComponent liftableWorldObjectComponent)
    {
        liftableWorldObjectComponent.Lifted += HandleLifted;
    }

    protected override void ComponentDetached(LiftableWorldObjectComponent liftableWorldObjectComponent)
    {
        liftableWorldObjectComponent.Lifted -= HandleLifted;
    }

    private void HandleLifted(LiftableWorldObjectComponent liftableWorldObjectComponent, Element element)
    {
        if(liftableWorldObjectComponent.Element is IComponents components)
        {
            if (!components.HasComponent<OwnerComponent>())
            {
                components.AddComponent(new OwnerComponent(element));
            }
        }
    }
}
