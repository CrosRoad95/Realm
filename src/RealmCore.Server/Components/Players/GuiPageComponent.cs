namespace RealmCore.Server.Components.Players;

public abstract class GuiPageComponent : Component
{
    private BlazorGuiComponent? _blazorGuiComponent;
    private readonly string _path;
    private readonly bool _isAsync;

    protected BlazorGuiComponent BlazorGuiComponent => _blazorGuiComponent ?? throw new InvalidOperationException();

    public GuiPageComponent(string path, bool isAsync = false)
    {
        _path = path;
        _isAsync = isAsync;
    }

    protected override void Load()
    {
        _blazorGuiComponent = Entity.GetRequiredComponent<BlazorGuiComponent>();
        BlazorGuiComponent.Open(_path, false, _isAsync);
    }

    public override void Dispose()
    {
        BlazorGuiComponent.Path = "index";
        BlazorGuiComponent.Visible = false;
        base.Dispose();
    }
}
