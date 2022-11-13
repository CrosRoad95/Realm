namespace Realm.Server.ResourcesLogic;

internal class LuaInteropLogic
{
    private readonly ILogger _logger;
    private readonly LuaInteropService _luaInteropService;

    public LuaInteropLogic(LuaInteropService luaInteropService, ILogger logger)
    {
        _luaInteropService = luaInteropService;
        _logger = logger.ForContext<LuaInteropLogic>();

        _luaInteropService.ClientErrorMessage += _luaInteropService_ClientErrorMessage;
    }

    private void _luaInteropService_ClientErrorMessage(Player player, string message, int level, string file, int line)
    {
        _logger.Warning("Clientside error on player {player} ({level}): {message} in {file}:{line}", (RPGPlayer)player, level, message, file, line);
    }
}
