namespace Realm.Server.ResourcesLogic;

internal class ClientUILogic
{
    private readonly EventScriptingFunctions _eventFunctions;
    private readonly FromLuaValueMapper _fromLuaValueMapper;

    public ClientUILogic(AgnosticGuiSystemService agnosticGuiSystemService, EventScriptingFunctions eventFunctions, FromLuaValueMapper fromLuaValueMapper)
    {
        _eventFunctions = eventFunctions;
        _fromLuaValueMapper = fromLuaValueMapper;

        agnosticGuiSystemService.FormSubmitted += FormSubmitted;
        agnosticGuiSystemService.GuiCloseRequested += GuiCloseRequested;
        agnosticGuiSystemService.GuiNavigationRequested += GuiNavigationRequested;
    }

    private void GuiNavigationRequested(LuaEvent luaEvent)
    {
        string? guiName = _fromLuaValueMapper.Map(typeof(string), luaEvent.Parameters[1]) as string;
        if (guiName == null)
            throw new NullReferenceException(nameof(guiName));

        var player = (RPGPlayer)luaEvent.Player;
        player.CloseAllGuis();
        player.OpenGui(guiName);
    }

    private void GuiCloseRequested(LuaEvent luaEvent)
    {
        var player = (RPGPlayer)luaEvent.Player;
        player.CloseAllGuis();
    }

    private async void FormSubmitted(LuaEvent luaEvent)
    {
        var formContext = new FormContextEvent(luaEvent, _fromLuaValueMapper);
        await _eventFunctions.InvokeEvent(formContext);
        luaEvent.Response(formContext.Id, formContext.Name, formContext.IsSuccess, formContext.Response);
    }
}
