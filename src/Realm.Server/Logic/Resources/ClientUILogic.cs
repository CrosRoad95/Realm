namespace Realm.Server.Logic.Resources;

internal class ClientUILogic
{
    private readonly EventScriptingFunctions _eventFunctions;
    private readonly FromLuaValueMapper _fromLuaValueMapper;

    public ClientUILogic(AgnosticGuiSystemService agnosticGuiSystemService, EventScriptingFunctions eventFunctions, FromLuaValueMapper fromLuaValueMapper)
    {
        _eventFunctions = eventFunctions;
        _fromLuaValueMapper = fromLuaValueMapper;

        agnosticGuiSystemService.FormSubmitted += HandleFormSubmitted;
        agnosticGuiSystemService.GuiCloseRequested += HandleGuiCloseRequested;
        agnosticGuiSystemService.GuiNavigationRequested += HandleGuiNavigationRequested;
    }

    private void HandleGuiNavigationRequested(LuaEvent luaEvent)
    {
        string? guiName = _fromLuaValueMapper.Map(typeof(string), luaEvent.Parameters[1]) as string;
        if (guiName == null)
            throw new NullReferenceException(nameof(guiName));

        var player = (RPGPlayer)luaEvent.Player;
        player.CloseAllGuis();
        player.OpenGui(guiName);
    }

    private void HandleGuiCloseRequested(LuaEvent luaEvent)
    {
        var player = (RPGPlayer)luaEvent.Player;
        player.CloseAllGuis();
    }

    private async void HandleFormSubmitted(LuaEvent luaEvent)
    {
        var formContext = new FormContextEvent(luaEvent, _fromLuaValueMapper);
        await _eventFunctions.InvokeEvent(formContext);
        luaEvent.Response(formContext.Id, formContext.Name, formContext.IsSuccess, formContext.Response);
    }
}
