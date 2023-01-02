using Realm.Domain.Contextes;
using SlipeServer.Server.Events;

namespace Realm.Domain.Components.Players;

public abstract class GuiComponent : Component
{
    [Inject]
    private AgnosticGuiSystemService AgnosticGuiSystemService { get; set; } = default!;
    protected readonly string _name;
    protected readonly bool _cursorless;

    public GuiComponent(string name, bool cursorless)
    {
        _name = name;
        _cursorless = cursorless;
    }

    public override async Task Load()
    {
        AgnosticGuiSystemService.FormSubmitted += HandleFormSubmitted;
        AgnosticGuiSystemService.ActionExecuted += HandleActionExecuted;
        await OpenGui();
    }

    protected virtual async Task OpenGui()
    {
        var playerElementComponent = Entity.GetRequiredComponent<PlayerElementComponent>();
        await AgnosticGuiSystemService.OpenGui(playerElementComponent.Player, _name, _cursorless);
    }

    private async void HandleActionExecuted(LuaEvent luaEvent)
    {
        //string id = luaEvent.Parameters[0].StringValue ?? throw new Exception();
        string guiName = luaEvent.Parameters[1].StringValue ?? throw new Exception();
        if (guiName == _name)
        {
            string actionName = luaEvent.Parameters[2].StringValue ?? throw new Exception();
            await HandleAction(new ActionContext(actionName, luaEvent.Parameters[3]));
        }
    }

    private async void HandleFormSubmitted(LuaEvent luaEvent)
    {
        //string id = luaEvent.Parameters[0].StringValue ?? throw new Exception();
        string guiName = luaEvent.Parameters[1].StringValue ?? throw new Exception();
        if(guiName == _name)
        {
            string formName = luaEvent.Parameters[2].StringValue ?? throw new Exception();
            await HandleForm(new FormContext(luaEvent.Player, formName, luaEvent.Parameters[3], AgnosticGuiSystemService));
        }
    }

    protected abstract Task HandleForm(IFormContext formContext);
    protected abstract Task HandleAction(IActionContext actionContext);

    public void Close()
    {
        AgnosticGuiSystemService.FormSubmitted -= HandleFormSubmitted;
        AgnosticGuiSystemService.ActionExecuted -= HandleActionExecuted;

        var playerElementComponent = Entity.GetRequiredComponent<PlayerElementComponent>();
        AgnosticGuiSystemService.CloseGui(playerElementComponent.Player, _name);
    }

    public override void Destroy()
    {
        Close();
    }
}
