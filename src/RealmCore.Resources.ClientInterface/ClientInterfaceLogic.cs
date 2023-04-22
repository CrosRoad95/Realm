using Microsoft.Extensions.Logging;
using RealmCore.Resources.Base;
using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Services;

namespace RealmCore.Resources.ClientInterface;

internal class ClientInterfaceLogic
{
    private readonly FromLuaValueMapper _fromLuaValueMapper;
    private readonly IClientInterfaceService _clientInterfaceService;
    private readonly ILogger<ClientInterfaceLogic> _logger;
    private readonly ILuaEventHub<IClientInterfaceEventHub> _luaEventHub;
    private readonly ClientInterfaceResource _resource;
    private readonly List<Element> _focusableElements = new();
    private readonly object _focusableElementsLock = new();

    public ClientInterfaceLogic(MtaServer server, LuaEventService luaEventService, FromLuaValueMapper fromLuaValueMapper,
        IClientInterfaceService clientInterfaceService, ILogger<ClientInterfaceLogic> logger, ILuaEventHub<IClientInterfaceEventHub> luaEventHub)
    {
        _fromLuaValueMapper = fromLuaValueMapper;
        _clientInterfaceService = clientInterfaceService;
        _logger = logger;
        _luaEventHub = luaEventHub;
        server.PlayerJoined += HandlePlayerJoin;

        _resource = server.GetAdditionalResource<ClientInterfaceResource>();
        luaEventService.AddEventHandler("internalDebugMessage", HandleInternalDebugMessage);
        luaEventService.AddEventHandler("sendLocalizationCode", HandleLocalizationCode);
        luaEventService.AddEventHandler("sendScreenSize", HandleScreenSize);
        luaEventService.AddEventHandler("internalChangeFocusedElement", HandleFocusedElementChanged);
        _clientInterfaceService.FocusableAdded += HandleFocusableAdded;
        _clientInterfaceService.FocusableRemoved += HandleFocusableRemoved;
        _clientInterfaceService.FocusableRenderingChanged += HandlePlayerFocusableRenderingEnabled;
    }

    private void HandleFocusableAdded(Element element)
    {
        bool added = false;
        lock (_focusableElementsLock)
            if (!_focusableElements.Contains(element))
            {
                _focusableElements.Add(element);
                element.Destroyed += HandleDestroyed;
                added = true;
            }

        if (added)
            _luaEventHub.Broadcast(x => x.AddFocusable(), element);
    }

    private void HandleDestroyed(Element element)
    {
        HandleFocusableRemoved(element);
    }

    private void HandleFocusableRemoved(Element element)
    {
        bool removed = false;
        lock (_focusableElementsLock)
            if (_focusableElements.Contains(element))
            {
                _focusableElements.Remove(element);
                element.Destroyed -= HandleDestroyed;
                removed = true;
            }

        if (removed)
            _luaEventHub.Broadcast(x => x.RemoveFocusable(), element);
    }

    private async void HandlePlayerJoin(Player player)
    {
        try
        {
            await _resource.StartForAsync(player);
            if (_focusableElements.Any())
                _luaEventHub.Invoke(player, x => x.AddFocusables(_focusableElements));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start ClientInterface resource for player: {playerName}, serial: {playerSerial}", player.Name, player.Client.Serial);
        }
    }

    private void HandlePlayerFocusableRenderingEnabled(Player player, bool enabled)
    {
        _luaEventHub.Invoke(player, x => x.SetFocusableRenderingEnabled(enabled));
    }

    private void HandleLocalizationCode(LuaEvent luaEvent)
    {
        var (id, code) = luaEvent.Read<string, string>(_fromLuaValueMapper);

        if (code != null)
        {
            _clientInterfaceService.BroadcastPlayerLocalizationCode(luaEvent.Player, code);
        }
        else
        {
            _logger.LogWarning("Failed to get localization code for player {player}", luaEvent.Player);
        }
    }

    private void HandleScreenSize(LuaEvent luaEvent)
    {
        var (id, x, y) = luaEvent.Read<string, int, int>(_fromLuaValueMapper);
        if (x != 0 && y != 0)
        {
            _clientInterfaceService.BroadcastPlayerScreenSize(luaEvent.Player, x, y);
        }
        else
        {
            _logger.LogWarning("Failed to get screen size for player {player}", luaEvent.Player);
        }
    }

    private void HandleInternalDebugMessage(LuaEvent luaEvent)
    {
        var (id, message, level, file, line) = luaEvent.Read<string, string, int, string, int>(_fromLuaValueMapper);
        _clientInterfaceService.BroadcastClientErrorMessage(luaEvent.Player, message, level, file, line);
    }

    private void HandleFocusedElementChanged(LuaEvent luaEvent)
    {
        var (id, focusedElement, childElement) = luaEvent.Read<string, Element, string>(_fromLuaValueMapper);

        _clientInterfaceService.BroadcastPlayerElementFocusChanged(luaEvent.Player, focusedElement, childElement);
    }
}
