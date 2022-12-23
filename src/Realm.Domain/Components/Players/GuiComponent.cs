using Realm.Domain.Components.Elements;
using Realm.Domain.Contextes;
using Realm.Resources.AgnosticGuiSystem;
using Serilog.Events;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Events;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Services;

namespace Realm.Domain.Components.Players;

public abstract class GuiComponent : Component
{
    private readonly string _name;
    public GuiComponent(string name)
    {
        _name = name;
    }

    public override async Task Load()
    {
        var agnosticGuiSystemService = Entity.GetRequiredService<AgnosticGuiSystemService>();
        agnosticGuiSystemService.FormSubmitted += HandleFormSubmitted;
        agnosticGuiSystemService.ActionExecuted += HandleActionExecuted;

        var playerElementComponent = Entity.GetRequiredComponent<PlayerElementComponent>();
        await agnosticGuiSystemService.OpenGui(playerElementComponent.Player, _name);
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
            await HandleForm(new FormContext(formName, luaEvent.Parameters[3]));
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
