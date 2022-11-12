using Serilog;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Services;

namespace Realm.Resources.LuaInterop;

internal class LuaInteropLogic
{
    private readonly FromLuaValueMapper _fromLuaValueMapper;
    private readonly LuaInteropService _luaInteropService;
    private readonly ILogger _logger;
    private readonly LuaInteropResource _resource;

    public LuaInteropLogic(MtaServer server, LuaEventService luaEventService, FromLuaValueMapper fromLuaValueMapper,
        LuaInteropService luaInteropService, ILogger logger)
    {
        _fromLuaValueMapper = fromLuaValueMapper;
        _luaInteropService = luaInteropService;
        _logger = logger.ForContext<LuaInteropLogic>();
        server.PlayerJoined += HandlePlayerJoin;

        _resource = server.GetAdditionalResource<LuaInteropResource>();
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
            _luaInteropService.BroadcastPlayerLocalizationCode(luaEvent.Player, code);
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
        _luaInteropService.BroadcastClientErrorMessage(luaEvent.Player, message, level, file, line);    
    }
}
