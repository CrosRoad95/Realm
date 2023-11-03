namespace RealmCore.Server.Components.Players.Abstractions;

public abstract class DxGuiComponent : GuiComponent
{
    protected readonly string _name;
    protected readonly bool _cursorLess;
    public string Name => _name;
    public bool CursorLess => _cursorLess;

    protected DxGuiComponent(string name, bool cursorLess)
    {
        _name = name;
        _cursorLess = cursorLess;
    }

    protected virtual Task HandleForm(IFormContext formContext) { return Task.CompletedTask; }
    protected virtual Task HandleAction(IActionContext actionContext) { return Task.CompletedTask; }

    internal Task InternalHandleForm(IFormContext formContext) => HandleForm(formContext);
    internal Task InternalHandleAction(IActionContext actionContext) => HandleAction(actionContext);
}
