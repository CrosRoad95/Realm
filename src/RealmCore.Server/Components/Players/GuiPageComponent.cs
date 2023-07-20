namespace RealmCore.Server.Components.Players;

public abstract class GuiPageComponent : Component
{
    private BlazorGuiComponent? _blazorGuiComponent;

    protected BlazorGuiComponent BlazorGuiComponent => _blazorGuiComponent ?? throw new InvalidOperationException();

    protected override void Load()
    {
        _blazorGuiComponent = Entity.GetRequiredComponent<BlazorGuiComponent>();
        base.Load();
        LoadGui();
    }

    protected abstract void LoadGui();

    public override void Dispose()
    {
        BlazorGuiComponent.Path = "index";
        BlazorGuiComponent.Visible = false;
        base.Dispose();
    }
}
