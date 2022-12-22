using Realm.Domain.Components.Elements;
using Realm.Domain.Contextes;
using Realm.Resources.AgnosticGuiSystem;
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

    public override Task Load()
    {
        var _agnosticGuiSystemService = Entity.GetRequiredService<AgnosticGuiSystemService>();
        var luaEventService = Entity.GetRequiredService<LuaEventService>();
        _agnosticGuiSystemService.FormSubmitted += HandleFormSubmitted;

        var playerElementComponent = Entity.GetRequiredComponent<PlayerElementComponent>();
        luaEventService.TriggerEventFor(playerElementComponent.Player, "internalUiOpenGui", playerElementComponent.Player, _name);
        return Task.CompletedTask;
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
}
