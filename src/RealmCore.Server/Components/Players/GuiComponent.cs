namespace RealmCore.Server.Components.Players;

public abstract class GuiComponent : Component
{
    [Inject]
    private IGuiSystemService GuiSystemService { get; set; } = default!;
    [Inject]
    private ILogger<GuiComponent> Logger { get; set; } = default!;
    [Inject]
    private IECS ECS { get; set; } = default!;
    [Inject]
    private FromLuaValueMapper FromLuaValueMapper { get; set; } = default!;
    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = default!;

    protected readonly string _name;
    protected readonly bool _cursorless;

    protected GuiComponent(string name, bool cursorless)
    {
        _name = name;
        _cursorless = cursorless;
    }

    protected override void Load()
    {
        GuiSystemService.FormSubmitted += HandleFormSubmitted;
        GuiSystemService.ActionExecuted += HandleActionExecuted;
        OpenGui();
    }

    protected virtual void OpenGui()
    {
        GuiSystemService.OpenGui(Entity.Player, _name, _cursorless);
    }

    private async void HandleActionExecuted(LuaEvent luaEvent)
    {
        try
        {
            var (id, guiName, actionName, data) = luaEvent.Read<string, string, string, LuaValue>(FromLuaValueMapper);
            if (guiName == _name)
            {
                try
                {
                    await HandleAction(new ActionContext(actionName, data));
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to execute action {actionName}.", actionName);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to read luaEvent parameters");
        }
    }

    private async void HandleFormSubmitted(LuaEvent luaEvent)
    {
        try
        {
            var (id, guiName, formName, data) = luaEvent.Read<string, string, string, LuaValue>(FromLuaValueMapper);
            if (guiName == _name)
            {
                var formContext = new FormContext(luaEvent.Player, formName, data, GuiSystemService, ECS, ServiceProvider);
                try
                {
                    await HandleForm(formContext);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to handle form {formName}.", formName);
                    formContext.ErrorResponse("Wystąpił nieznany błąd.");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to read luaEvent parameters");
        }
    }

    public void Close()
    {
        ThrowIfDisposed();

        GuiSystemService.FormSubmitted -= HandleFormSubmitted;
        GuiSystemService.ActionExecuted -= HandleActionExecuted;

        var playerElementComponent = Entity.GetRequiredComponent<PlayerElementComponent>();
        GuiSystemService.CloseGui(playerElementComponent.Player, _name, _cursorless);
    }

    public override void Dispose()
    {
        Close();
        base.Dispose();
    }

    protected virtual Task HandleForm(IFormContext formContext)
    {
        throw new NotImplementedException();
    }

    protected virtual Task HandleAction(IActionContext actionContext)
    {
        throw new NotImplementedException();
    }
}
