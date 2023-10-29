namespace RealmCore.Server.Components.Players.Abstractions;

public abstract class DxGuiComponent : GuiComponent
{
    protected readonly string _name;
    protected readonly bool _cursorless;
    public string Name => _name;
    public bool Cursorless => _cursorless;

    protected DxGuiComponent(string name, bool cursorless)
    {
        _name = name;
        _cursorless = cursorless;
    }

    protected virtual Task HandleForm(IFormContext formContext) { return Task.CompletedTask; }
    protected virtual Task HandleAction(IActionContext actionContext) { return Task.CompletedTask; }

    internal Task InternalHandleForm(IFormContext formContext) => HandleForm(formContext);
    internal Task InternalHandleAction(IActionContext actionContext) => HandleAction(actionContext);
}
