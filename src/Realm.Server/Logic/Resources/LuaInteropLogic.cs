namespace Realm.Server.Logic.Resources;

internal class LuaInteropLogic
{
    private readonly ILogger _logger;
    private readonly LuaInteropService _luaInteropService;

    public LuaInteropLogic(MtaServer mtaServer, LuaInteropService luaInteropService, ILogger logger)
    {
        _luaInteropService = luaInteropService;
        _logger = logger.ForContext<LuaInteropLogic>();

        _luaInteropService.ClientErrorMessage += HandleClientErrorMessage;
    }

    private void HandleClientErrorMessage(Player player, string message, int level, string file, int line)
    {
        switch (level)
        {
            case 0: // Custom
            case 3: // Information
                _logger.Information("Clientside: {player} ({level}): {message} in {file}:{line}", player, level, message, file, line);
                break;
            case 2: // Warning
                _logger.Warning("Clientside: {player} ({level}): {message} in {file}:{line}", player, level, message, file, line);
                break;
            default: // Error or something else
                _logger.Error("Clientside: {player} ({level}): {message} in {file}:{line}", player, level, message, file, line);
                break;
        }
    }
}
