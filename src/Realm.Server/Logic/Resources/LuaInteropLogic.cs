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
        mtaServer.PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(Player player)
    {
        ((RPGPlayer)player).ClipboardChanged += HandleClipboardSet;
    }

    private void HandleClipboardSet(RPGPlayer rpgPlayer, string content)
    {
        _luaInteropService.SetClipboard(rpgPlayer, content);
    }

    private void HandleClientErrorMessage(Player player, string message, int level, string file, int line)
    {
        var rpgPlayer = (RPGPlayer)player;
        switch (level)
        {
            case 0: // Custom
            case 3: // Information
                _logger.Information("Clientside: {player} ({level}): {message} in {file}:{line}", rpgPlayer, level, message, file, line);
                break;
            case 2: // Warning
                _logger.Warning("Clientside: {player} ({level}): {message} in {file}:{line}", rpgPlayer, level, message, file, line);
                break;
            default: // Error or something else
                _logger.Error("Clientside: {player} ({level}): {message} in {file}:{line}", rpgPlayer, level, message, file, line);
                break;
        }
    }
}
