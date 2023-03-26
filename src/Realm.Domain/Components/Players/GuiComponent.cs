using Realm.Domain.Contexts;

namespace Realm.Domain.Components.Players;

public abstract class GuiComponent : Component
{
    [Inject]
    private IAgnosticGuiSystemService AgnosticGuiSystemService { get; set; } = default!;
    [Inject]
    private ILogger<GuiComponent> Logger { get; set; } = default!;
    [Inject]
    private IECS ECS { get; set; } = default!;

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
        OpenGui();
    }

    protected virtual void OpenGui()
    {
        AgnosticGuiSystemService.OpenGui(Entity.Player, _name, _cursorless);
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
                Logger.LogError(ex, "Failed to execute action {actionName}.", actionName);
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
                await HandleForm(new FormContext(luaEvent.Player, formName, luaEvent.Parameters[3], AgnosticGuiSystemService, ECS));
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "Failed to handle form {formName}.", formName);
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
