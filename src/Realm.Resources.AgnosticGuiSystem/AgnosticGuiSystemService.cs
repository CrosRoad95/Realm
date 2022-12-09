using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;

namespace Realm.Resources.AgnosticGuiSystem;

public class AgnosticGuiSystemService
{
    public event Action<LuaEvent>? FormSubmitted;
    public event Action<LuaEvent>? GuiCloseRequested;
    public event Action<LuaEvent>? GuiNavigationRequested;
    private Dictionary<Player, HashSet<string>> _playersGuis = new();
    public AgnosticGuiSystemService()
    {

    }

    internal void HandleInternalSubmitForm(LuaEvent luaEvent)
    {
        FormSubmitted?.Invoke(luaEvent);
    }

    internal void HandleInternalRequestGuiClose(LuaEvent luaEvent)
    {
        GuiCloseRequested?.Invoke(luaEvent);
    }

    internal void HandleInternalNavigateToGui(LuaEvent luaEvent)
    {
        GuiNavigationRequested?.Invoke(luaEvent);
    }

    private void EnsurePlayerGuisAreInitialized(Player player)
    {
        if (!_playersGuis.ContainsKey(player))
            _playersGuis[player] = new();
    }

    public bool OpenGui(Player player, string gui)
    {
        EnsurePlayerGuisAreInitialized(player);

        if (_playersGuis[player].Contains(gui))
            return false;

        _playersGuis[player].Add(gui);
        player.TriggerLuaEvent("internalUiOpenGui", player, gui);
        return true;
    }

    public bool CloseGui(Player player, string gui)
    {
        EnsurePlayerGuisAreInitialized(player);

        if (!_playersGuis[player].Contains(gui))
            return false;

        _playersGuis[player].Remove(gui);
        player.TriggerLuaEvent("internalUiCloseGui", player, gui);
        return true;
    }

    public void CloseAllGuis(Player player)
    {
        EnsurePlayerGuisAreInitialized(player);

        foreach (var gui in _playersGuis[player])
            player.TriggerLuaEvent("internalUiCloseGui", player, gui);
    }
}
