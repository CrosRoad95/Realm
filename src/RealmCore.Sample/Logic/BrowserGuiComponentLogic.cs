namespace RealmCore.Sample.Logic;

internal sealed class BrowserGuiComponentLogic : ComponentLogic<BrowserGuiComponent>
{
    public BrowserGuiComponentLogic(IECS ecs) : base(ecs)
    {
    }

    protected override void ComponentAdded(BrowserGuiComponent blazorGuiComponent)
    {
        blazorGuiComponent.LoadRemotePage("counter");
    }
}
