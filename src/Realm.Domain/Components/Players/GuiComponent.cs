namespace Realm.Domain.Components.Players;

public abstract class GuiComponent : Component
{
    [Inject]
    private AgnosticGuiSystemService AgnosticGuiSystemService { get; set; } = default!;
    [Inject]
    private ILogger Logger { get; set; } = default!;

    protected readonly string _name;
    protected readonly bool _cursorless;

    protected GuiComponent(string name, bool cursorless)
    {
        _name = name;
        _cursorless = cursorless;
    }

    protected override void Load()
    {
        AgnosticGuiSystemService.FormSubmitted += HandleFormSubmitted;
        AgnosticGuiSystemService.ActionExecuted += HandleActionExecuted;
    }

    protected override async Task LoadAsync()
    {
        await OpenGui();
    }

    protected virtual async Task OpenGui()
    {
        var playerElementComponent = Entity.GetRequiredComponent<PlayerElementComponent>();
        await AgnosticGuiSystemService.OpenGui(playerElementComponent.Player, _name, _cursorless);
    }

    private async void HandleActionExecuted(LuaEvent luaEvent)
    {
        //string id = luaEvent.Parameters[0].StringValue ?? throw new InvalidOperationException();
        string guiName = luaEvent.Parameters[1].StringValue ?? throw new InvalidOperationException();
        if (guiName == _name)
        {
            string actionName = luaEvent.Parameters[2].StringValue ?? throw new InvalidOperationException();
            try
            {
                await HandleAction(new ActionContext(actionName, luaEvent.Parameters[3]));
            }
            catch(Exception ex)
            {
                Logger.Error(ex, "Failed to execute action {actionName}.", actionName);
            }
        }
    }

    private async void HandleFormSubmitted(LuaEvent luaEvent)
    {
        //string id = luaEvent.Parameters[0].StringValue ?? throw new InvalidOperationException();
        string guiName = luaEvent.Parameters[1].StringValue ?? throw new InvalidOperationException();
        if(guiName == _name)
        {
            string formName = luaEvent.Parameters[2].StringValue ?? throw new InvalidOperationException();
            try
            {
                await HandleForm(new FormContext(luaEvent.Player, formName, luaEvent.Parameters[3], AgnosticGuiSystemService));
            }
            catch(Exception ex)
            {
                Logger.Error(ex, "Failed to handle form {formName}.", formName);
            }
        }
    }

    public void Close()
    {
        AgnosticGuiSystemService.FormSubmitted -= HandleFormSubmitted;
        AgnosticGuiSystemService.ActionExecuted -= HandleActionExecuted;

        var playerElementComponent = Entity.GetRequiredComponent<PlayerElementComponent>();
        AgnosticGuiSystemService.CloseGui(playerElementComponent.Player, _name, _cursorless);
    }

    public override void Dispose()
    {
        base.Dispose();
        Close();
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
