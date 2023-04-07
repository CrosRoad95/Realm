using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Mappers;
using System.Collections.Concurrent;

namespace RealmCore.Resources.AgnosticGuiSystem;

internal sealed class AgnosticGuiSystemService : IAgnosticGuiSystemService
{
    public GuiChangedDelegate? GuiFilesChanged { get; set; }

    private readonly AgnosticGuiSystemResource _resource;
    public event Action<LuaEvent>? FormSubmitted;
    public event Action<LuaEvent>? ActionExecuted;
    private readonly ConcurrentDictionary<Player, object> _playersLocks = new();
    private readonly ConcurrentDictionary<Player, List<string>> _playersGuis = new();
    private readonly LuaValueMapper _luaValueMapper;

    public AgnosticGuiSystemService(MtaServer server, LuaValueMapper luaValueMapper)
    {
        _resource = server.GetAdditionalResource<AgnosticGuiSystemResource>();
        _luaValueMapper = luaValueMapper;
    }

    public void HandleInternalSubmitForm(LuaEvent luaEvent)
    {
        FormSubmitted?.Invoke(luaEvent);
    }

    public void HandleInternalActionExecuted(LuaEvent luaEvent)
    {
        ActionExecuted?.Invoke(luaEvent);
    }

    private void EnsurePlayerGuisAreInitialized(Player player)
    {
        _playersGuis.GetOrAdd(player, x => new());
        _playersLocks.GetOrAdd(player, x => new());
    }

    public bool OpenGui(Player player, string gui, bool cursorless, LuaValue? arg1 = null)
    {
        EnsurePlayerGuisAreInitialized(player);

        lock (_playersLocks[player])
        {
            if (_playersGuis[player].Contains(gui))
                return false;

            _playersGuis[player].Add(gui);
            player.TriggerLuaEvent("internalUiOpenGui", player, gui, cursorless, arg1 ?? new LuaValue());
        }
        return true;
    }

    public bool CloseGui(Player player, string gui, bool cursorless)
    {
        EnsurePlayerGuisAreInitialized(player);

        lock (_playersLocks[player])
        {
            if (!_playersGuis[player].Contains(gui))
                return false;

            _playersGuis[player].Remove(gui);
            player.TriggerLuaEvent("internalUiCloseGui", player, gui, cursorless);
        }
        return true;
    }

    public void CloseAllGuis(Player player)
    {
        EnsurePlayerGuisAreInitialized(player);

        lock (_playersLocks[player])
        {
            foreach (var gui in _playersGuis[player])
                player.TriggerLuaEvent("internalUiCloseGui", player, gui, false);
            _playersGuis[player].Clear();
        }
    }

    public void SetDebugToolsEnabled(Player player, bool enabled)
    {
        player.TriggerLuaEvent("internalUiDebugToolsEnabled", player, enabled);
    }

    public void SendFormResponse(Player player, string id, string name, params object[] values)
    {
        EnsurePlayerGuisAreInitialized(player);

        var luaValues = values.Select(_luaValueMapper.Map).ToArray();
        player.TriggerLuaEvent("internalSubmitFormResponse", player, id, name, luaValues);
    }

    public void SendStateChanged(Player player, string guiName, Dictionary<LuaValue, LuaValue> changes)
    {
        EnsurePlayerGuisAreInitialized(player);

        player.TriggerLuaEvent("internalUiStateChanged", player, guiName, new LuaValue(changes));
    }

    public async Task UpdateGuiFiles()
    {
        _resource.UpdateGuiFiles();
        var players = _playersGuis.Keys;
        foreach (var player in players)
        {
            CloseAllGuis(player);
            _resource.StopFor(player);
            await _resource.StartForAsync(player);
        }

        if (GuiFilesChanged != null)
            await GuiFilesChanged();
    }
}
