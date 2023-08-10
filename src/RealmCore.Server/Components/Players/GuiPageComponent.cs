namespace RealmCore.Server.Components.Players;

public abstract class GuiPageComponent : Component
{
    private BlazorGuiComponent? _blazorGuiComponent;

    protected BlazorGuiComponent BlazorGuiComponent => _blazorGuiComponent ?? throw new InvalidOperationException();

    public string Path { get; }
    public bool IsAsync { get; }

    public GuiPageComponent(string path, bool isAsync = false)
    {
        Path = path;
        IsAsync = isAsync;
    }

    protected override void Load()
    {
        _blazorGuiComponent = Entity.GetRequiredComponent<BlazorGuiComponent>();
        BlazorGuiComponent.Open(Path, false, IsAsync);
    }

    public override void Dispose()
    {
        BlazorGuiComponent.Path = "index";
        BlazorGuiComponent.Visible = false;
        base.Dispose();
    }
}
