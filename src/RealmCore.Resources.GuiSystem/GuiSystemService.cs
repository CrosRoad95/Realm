using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Mappers;
using System.Collections.Concurrent;

namespace RealmCore.Resources.GuiSystem;

internal sealed class GuiSystemService : IGuiSystemService
{
    public GuiChangedDelegate? GuiFilesChanged { get; set; }

    public event Action<LuaEvent>? FormSubmitted;
    public event Action<LuaEvent>? ActionExecuted;
    private readonly ConcurrentDictionary<Player, object> _playersLocks = new();
    private readonly ConcurrentDictionary<Player, List<string>> _playersGuis = new();
    private readonly MtaServer _server;
    private readonly LuaValueMapper _luaValueMapper;

    public GuiSystemService(MtaServer server, LuaValueMapper luaValueMapper)
    {
        _server = server;
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

    private void EnsurePlayerGuiAreInitialized(Player player)
    {
        _playersGuis.GetOrAdd(player, x => new());
        _playersLocks.GetOrAdd(player, x => new());
    }

    public bool OpenGui(Player player, string gui, bool cursorLess, LuaValue? arg1 = null)
    {
        EnsurePlayerGuiAreInitialized(player);

        lock (_playersLocks[player])
        {
            if (_playersGuis[player].Contains(gui))
                return false;

            _playersGuis[player].Add(gui);
            player.TriggerLuaEvent("internalUiOpenGui", player, gui, cursorLess, arg1 ?? new LuaValue());
        }
        return true;
    }

    public bool CloseGui(Player player, string gui, bool cursorless)
    {
        EnsurePlayerGuiAreInitialized(player);

        lock (_playersLocks[player])
        {
            if (!_playersGuis[player].Contains(gui))
                return false;

            _playersGuis[player].Remove(gui);
            player.TriggerLuaEvent("internalUiCloseGui", player, gui, cursorless);
        }
        return true;
    }

    public void CloseAllGui(Player player)
    {
        EnsurePlayerGuiAreInitialized(player);

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
        EnsurePlayerGuiAreInitialized(player);

        var luaValues = values.Select(_luaValueMapper.Map).ToArray();
        player.TriggerLuaEvent("internalSubmitFormResponse", player, id, name, luaValues);
    }

    public void SendStateChanged(Player player, string guiName, Dictionary<LuaValue, LuaValue> changes)
    {
        EnsurePlayerGuiAreInitialized(player);

        player.TriggerLuaEvent("internalUiStateChanged", player, guiName, new LuaValue(changes));
    }

    public async Task UpdateGuiFiles()
    {
        var resource = _server.GetAdditionalResource<GuiSystemResource>();
        resource.UpdateGuiFiles();
        var players = _playersGuis.Keys;
        foreach (var player in players)
        {
            CloseAllGui(player);
            resource.StopFor(player);
            await resource.StartForAsync(player);
        }

        if (GuiFilesChanged != null)
            await GuiFilesChanged();
    }
}
