namespace RealmCore.Sample.Logic;

internal sealed class BrowserGuiComponentLogic : ComponentLogic<BrowserComponent>
{
    public BrowserGuiComponentLogic(IEntityEngine ecs) : base(ecs)
    {
    }

    protected override void ComponentAdded(BrowserComponent blazorGuiComponent)
    {
        //blazorGuiComponent.LoadRemotePage("counter");
    }
}
