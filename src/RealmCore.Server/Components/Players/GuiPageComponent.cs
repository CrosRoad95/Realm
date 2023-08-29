namespace RealmCore.Server.Components.Players;

public abstract class GuiPageComponent : Component
{
    private BrowserGuiComponent? _blazorGuiComponent;

    protected BrowserGuiComponent BlazorGuiComponent => _blazorGuiComponent ?? throw new InvalidOperationException();

    public string Path { get; }
    public bool IsAsync { get; }
    private bool _loaded;

    public GuiPageComponent(string path, bool isAsync = false)
    {
        Path = path;
        IsAsync = isAsync;
    }

    protected override void Attach()
    {
        _blazorGuiComponent = Entity.GetRequiredComponent<BrowserGuiComponent>();
        BlazorGuiComponent.Open(Path, false, IsAsync);
        _loaded = true;
    }

    public override void Dispose()
    {
        if(_loaded)
        {
            BlazorGuiComponent.Path = "index";
            BlazorGuiComponent.Visible = false;
        }
        base.Dispose();
    }
}
