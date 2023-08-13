namespace RealmCore.Server.Components.Players;

public abstract class GuiPageComponent : Component
{
    private BlazorGuiComponent? _blazorGuiComponent;

    protected BlazorGuiComponent BlazorGuiComponent => _blazorGuiComponent ?? throw new InvalidOperationException();

    public string Path { get; }
    public bool IsAsync { get; }
    private bool _loaded;

    public GuiPageComponent(string path, bool isAsync = false)
    {
        Path = path;
        IsAsync = isAsync;
    }

    protected override void Load()
    {
        _blazorGuiComponent = Entity.GetRequiredComponent<BlazorGuiComponent>();
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
