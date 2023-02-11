using Microsoft.Extensions.Logging;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Services;

namespace Realm.Resources.ClientInterface;

internal class ClientInterfaceLogic
{
    private readonly LuaEventService _luaEventService;
    private readonly FromLuaValueMapper _fromLuaValueMapper;
    private readonly ClientInterfaceService _clientInterfaceService;
    private readonly ILogger<ClientInterfaceLogic> _logger;
    private readonly ClientInterfaceResource _resource;
    private readonly List<Element> _focusableElements = new();
    private readonly object _focusableElementsLock = new();

    public ClientInterfaceLogic(MtaServer server, LuaEventService luaEventService, FromLuaValueMapper fromLuaValueMapper,
        ClientInterfaceService ClientInterfaceService, ILogger<ClientInterfaceLogic> logger)
    {
        _luaEventService = luaEventService;
        _fromLuaValueMapper = fromLuaValueMapper;
        _clientInterfaceService = ClientInterfaceService;
        _logger = logger;
        server.PlayerJoined += HandlePlayerJoin;

        _resource = server.GetAdditionalResource<ClientInterfaceResource>();
        luaEventService.AddEventHandler("internalDebugMessage", HandleInternalDebugMessage);
        luaEventService.AddEventHandler("sendLocalizationCode", HandleLocalizationCode);
        luaEventService.AddEventHandler("sendScreenSize", HandleScreenSize);
        luaEventService.AddEventHandler("internalChangeFocusedElement", HandleFocusedElementChanged);
        _clientInterfaceService.FocusableAdded += HandleFocusableAdded;
        _clientInterfaceService.FocusableRemoved += HandleFocusableRemoved;
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
            _luaEventService.TriggerEvent("internalAddFocusable", element);
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
            _luaEventService.TriggerEvent("internalRemoveFocusable", element);
    }

    private async void HandlePlayerJoin(Player player)
    {
        await _resource.StartForAsync(player);
        if(_focusableElements.Any())
            _luaEventService.TriggerEvent("internalAddFocusables", player, _focusableElements);
    }

    private void HandleLocalizationCode(LuaEvent luaEvent)
    {
        var code = luaEvent.Parameters[1].StringValue;
        if(code != null)
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
        var x = luaEvent.Parameters[1].IntegerValue;
        var y = luaEvent.Parameters[2].IntegerValue;
        if(x != null && y != null)
        {
            _clientInterfaceService.BroadcastPlayerScreenSize(luaEvent.Player, x.Value, y.Value);
        }
        else
        {
            _logger.LogWarning("Failed to get screen size for player {player}", luaEvent.Player);
        }
    }

    private void HandleInternalDebugMessage(LuaEvent luaEvent)
    {
        var message = _fromLuaValueMapper.Map(typeof(string), luaEvent.Parameters[1]) as string;
        var level = (int)_fromLuaValueMapper.Map(typeof(int), luaEvent.Parameters[2]);
        var file = _fromLuaValueMapper.Map(typeof(string), luaEvent.Parameters[3]) as string;
        var line = (int)_fromLuaValueMapper.Map(typeof(int), luaEvent.Parameters[4]);
        _clientInterfaceService.BroadcastClientErrorMessage(luaEvent.Player, message, level, file, line);    
    }

    private void HandleFocusedElementChanged(LuaEvent luaEvent)
    {
        var focusedElement = _fromLuaValueMapper.Map(typeof(Element), luaEvent.Parameters[1]) as Element;
        var childElement = _fromLuaValueMapper.Map(typeof(string), luaEvent.Parameters[2]) as string;
        if(childElement != null)
        {
            Console.WriteLine("FOCUS CHILD ELEMENT {0}", childElement);
            ;
        }
        _clientInterfaceService.BroadcastPlayerElementFocusChanged(luaEvent.Player, focusedElement);
    }
}
