namespace RealmCore.Resources.ClientInterface;

internal class ClientInterfaceLogic
{
    private readonly FromLuaValueMapper _fromLuaValueMapper;
    private readonly IClientInterfaceService _clientInterfaceService;
    private readonly ILogger<ClientInterfaceLogic> _logger;
    private readonly ILuaEventHub<IClientInterfaceEventHub> _luaEventHub;
    private readonly ClientInterfaceResource _resource;
    private readonly List<Element> _focusableElements = [];
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
        luaEventService.AddEventHandler("sendDebugMessagesBuffer", HandleDebugMessagesBuffer);
        luaEventService.AddEventHandler("sendLocalizationCode", HandleLocalizationCode);
        luaEventService.AddEventHandler("sendScreenSize", HandleScreenSize);
        luaEventService.AddEventHandler("internalChangeFocusedElement", HandleFocusedElementChanged);
        luaEventService.AddEventHandler("clickedElementChanged", HandleClickedElementChanged);
        _clientInterfaceService.FocusableRenderingChanged += HandlePlayerFocusableRenderingEnabled;
        _clientInterfaceService.FocusableAdded += HandleFocusableAdded;
        _clientInterfaceService.FocusableRemoved += HandleFocusableRemoved;
        _clientInterfaceService.FocusableForAdded += HandleFocusableForAdded;
        _clientInterfaceService.FocusableForRemoved += HandleFocusableForRemoved;
    }

    private void HandleFocusableForRemoved(Element element, Player player)
    {
        _luaEventHub.Invoke(player, x => x.RemoveFocusable(), element);
    }

    private void HandleFocusableForAdded(Element element, Player player)
    {
        _luaEventHub.Invoke(player, x => x.AddFocusable(), element);
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
            if (_focusableElements.Count != 0)
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
            _clientInterfaceService.RelayPlayerLocalizationCode(luaEvent.Player, code);
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
            _clientInterfaceService.RelayPlayerScreenSize(luaEvent.Player, x, y);
        }
        else
        {
            _logger.LogWarning("Failed to get screen size for player {player}", luaEvent.Player);
        }
    }

    private void HandleDebugMessagesBuffer(LuaEvent luaEvent)
    {
        if (luaEvent.Parameters.Length != 2)
            return;

        var debugMessages = luaEvent.Parameters[1].TableValue;
        if (debugMessages == null || debugMessages.Count == 0)
            return;

        var clientDebugMessages = new ClientDebugMessage[int.Min(128, debugMessages.Count)];
        for (int i = 0; i < clientDebugMessages.Length; i++)
        {
            var dm = debugMessages[i + 1].TableValue;
            if (dm == null)
                return;

            if (dm.Count == 2)
            {
                clientDebugMessages[i] = new ClientDebugMessage(dm[1].StringValue!, 0, "", 0);
            }
            else if (dm.Count == 4)
            {
                clientDebugMessages[i] = new ClientDebugMessage(dm[1].StringValue!, dm[2].IntegerValue.Value, dm[3].StringValue!, dm[4].IntegerValue.Value);
            }
        }

        _clientInterfaceService.RelayClienDebugMessages(luaEvent.Player, clientDebugMessages);
    }

    private void HandleFocusedElementChanged(LuaEvent luaEvent)
    {
        var (id, focusedElement, childElement) = luaEvent.Read<string, Element, string>(_fromLuaValueMapper);

        _clientInterfaceService.RelayPlayerElementFocusChanged(luaEvent.Player, focusedElement, childElement);
    }

    private void HandleClickedElementChanged(LuaEvent luaEvent)
    {
        var (id, clickedElement) = luaEvent.Read<string, Element>(_fromLuaValueMapper);

        _clientInterfaceService.RelayClickedElementChanged(luaEvent.Player, clickedElement);
    }
}
