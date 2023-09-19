namespace RealmCore.Server.Logic.Components;

internal sealed class LiftableWorldObjectComponentLogic : ComponentLogic<LiftableWorldObjectComponent>
{
    public LiftableWorldObjectComponentLogic(IEntityEngine entityEngine) : base(entityEngine)
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

    private void HandleLifted(LiftableWorldObjectComponent liftableWorldObjectComponent, Entity entity)
    {
        if (!liftableWorldObjectComponent.Entity.HasComponent<OwnerComponent>())
        {
            liftableWorldObjectComponent.Entity.AddComponent(new OwnerComponent(entity));
        }
    }
}
