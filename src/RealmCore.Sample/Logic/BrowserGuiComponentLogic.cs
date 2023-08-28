namespace RealmCore.Sample.Logic;

internal sealed class BrowserGuiComponentLogic : ComponentLogic<BrowserGuiComponent>
{
    public BrowserGuiComponentLogic(IEntityEngine ecs) : base(ecs)
    {
    }

    protected override void ComponentAdded(BrowserGuiComponent blazorGuiComponent)
    {
        //blazorGuiComponent.LoadRemotePage("counter");
    }
}
