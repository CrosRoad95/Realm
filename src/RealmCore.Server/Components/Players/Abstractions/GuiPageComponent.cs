namespace RealmCore.Server.Components.Players.Abstractions;

public abstract class GuiPageComponent : Component
{
    private BrowserComponent? _blazorGuiComponent;

    protected BrowserComponent BlazorGuiComponent => _blazorGuiComponent ?? throw new InvalidOperationException();

    public string Path { get; }
    public bool IsGuiAsync { get; }
    private bool _loaded;

    public GuiPageComponent(string path, bool isAsync = false)
    {
        Path = path;
        IsGuiAsync = isAsync;
    }

    protected override void Attach()
    {
        _blazorGuiComponent = Entity.GetRequiredComponent<BrowserComponent>();
        BlazorGuiComponent.Open(Path, false, IsGuiAsync);
        _loaded = true;
    }

    public override void Dispose()
    {
        if (_loaded)
        {
            BlazorGuiComponent.Path = "index";
            BlazorGuiComponent.Visible = false;
        }
        base.Dispose();
    }
}
