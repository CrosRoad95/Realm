using Realm.Domain.Components.Elements;
using Realm.Domain.Contextes;
using Realm.Resources.AgnosticGuiSystem;
using SlipeServer.Server.Events;

namespace Realm.Domain.Components.Players;

public abstract class GuiComponent : Component
{
    protected readonly string _name;
    protected readonly bool _cursorless;

    public GuiComponent(string name, bool cursorless)
    {
        _name = name;
        _cursorless = cursorless;
    }

    public override async Task Load()
    {
        var agnosticGuiSystemService = Entity.GetRequiredService<AgnosticGuiSystemService>();
        agnosticGuiSystemService.FormSubmitted += HandleFormSubmitted;
        agnosticGuiSystemService.ActionExecuted += HandleActionExecuted;
        await OpenGui();
    }

    protected virtual async Task OpenGui()
    {
        var agnosticGuiSystemService = Entity.GetRequiredService<AgnosticGuiSystemService>();
        var playerElementComponent = Entity.GetRequiredComponent<PlayerElementComponent>();
        await agnosticGuiSystemService.OpenGui(playerElementComponent.Player, _name, _cursorless);
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
            await HandleForm(new FormContext(luaEvent.Player, formName, luaEvent.Parameters[3], Entity.GetRequiredService<AgnosticGuiSystemService>()));
        }
    }

    protected abstract Task HandleForm(IFormContext formContext);
    protected abstract Task HandleAction(IActionContext actionContext);

    public void Close()
    {
        var agnosticGuiSystemService = Entity.GetRequiredService<AgnosticGuiSystemService>();
        agnosticGuiSystemService.FormSubmitted -= HandleFormSubmitted;
        agnosticGuiSystemService.ActionExecuted -= HandleActionExecuted;

        var playerElementComponent = Entity.GetRequiredComponent<PlayerElementComponent>();
        agnosticGuiSystemService.CloseGui(playerElementComponent.Player, _name);
    }

    public override void Destroy()
    {
        Close();
    }
}
