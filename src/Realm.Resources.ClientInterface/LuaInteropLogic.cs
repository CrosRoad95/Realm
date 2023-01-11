using Serilog;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Services;

namespace Realm.Resources.ClientInterface;

internal class ClientInterfaceLogic
{
    private readonly FromLuaValueMapper _fromLuaValueMapper;
    private readonly ClientInterfaceService _ClientInterfaceService;
    private readonly ILogger _logger;
    private readonly ClientInterfaceResource _resource;

    public ClientInterfaceLogic(MtaServer server, LuaEventService luaEventService, FromLuaValueMapper fromLuaValueMapper,
        ClientInterfaceService ClientInterfaceService, ILogger logger)
    {
        _fromLuaValueMapper = fromLuaValueMapper;
        _ClientInterfaceService = ClientInterfaceService;
        _logger = logger.ForContext<ClientInterfaceLogic>();
        server.PlayerJoined += HandlePlayerJoin;

        _resource = server.GetAdditionalResource<ClientInterfaceResource>();
        luaEventService.AddEventHandler("internalDebugMessage", HandleInternalDebugMessage);
        luaEventService.AddEventHandler("sendLocalizationCode", HandleLocalizationCode);
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }

    private void HandleLocalizationCode(LuaEvent luaEvent)
    {
        var code = luaEvent.Parameters[1].StringValue;
        if(code != null)
        {
            _ClientInterfaceService.BroadcastPlayerLocalizationCode(luaEvent.Player, code);
        }
        else
        {
            _logger.Warning("Failed to get localization code for player {player}", luaEvent.Player);
        }
    }

    private void HandleInternalDebugMessage(LuaEvent luaEvent)
    {
        var message = _fromLuaValueMapper.Map(typeof(string), luaEvent.Parameters[1]) as string;
        var level = (int)_fromLuaValueMapper.Map(typeof(int), luaEvent.Parameters[2]);
        var file = _fromLuaValueMapper.Map(typeof(string), luaEvent.Parameters[3]) as string;
        var line = (int)_fromLuaValueMapper.Map(typeof(int), luaEvent.Parameters[4]);
        _ClientInterfaceService.BroadcastClientErrorMessage(luaEvent.Player, message, level, file, line);    
    }
}
