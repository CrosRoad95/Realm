namespace Realm.Server.ResourcesLogic;

internal class ClientUILogic
{
    private readonly EventFunctions _eventFunctions;
    private readonly FromLuaValueMapper _fromLuaValueMapper;
    private readonly List<RPGPlayer> _startedForPlayers = new();

    public ClientUILogic(AgnosticGuiSystemService agnosticGuiSystemService, EventFunctions eventFunctions, FromLuaValueMapper fromLuaValueMapper)
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
        player.OpenGui(guiName);
    }

    private void GuiCloseRequested(LuaEvent luaEvent)
    {
        var player = (RPGPlayer)luaEvent.Player;
        player.CloseCurrentGui();
    }

    private async void FormSubmitted(LuaEvent luaEvent)
    {
        var formContext = new FormContext(luaEvent, _fromLuaValueMapper);
        await _eventFunctions.InvokeEvent(formContext);
        luaEvent.Response(formContext.Id, formContext.Name, formContext.IsSuccess, formContext.Response);
    }

    private void Player_Disconnected(Player player, PlayerQuitEventArgs e)
    {
        _startedForPlayers.Remove((RPGPlayer)player);
    }
}
