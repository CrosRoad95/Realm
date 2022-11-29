namespace Realm.Server.Logic.Resources;

internal class LuaInteropLogic
{
    private readonly ILogger _logger;
    private readonly LuaInteropService _luaInteropService;

    public LuaInteropLogic(MtaServer mtaServer, LuaInteropService luaInteropService, ILogger logger)
    {
        _luaInteropService = luaInteropService;
        _logger = logger.ForContext<LuaInteropLogic>();

        _luaInteropService.ClientErrorMessage += _luaInteropService_ClientErrorMessage;
        mtaServer.PlayerJoined += MtaServer_PlayerJoined;
    }

    private void MtaServer_PlayerJoined(Player player)
    {
        ((RPGPlayer)player).ClipboardChanged += LuaInteropLogic_ClipboardSet;
    }

    private void LuaInteropLogic_ClipboardSet(RPGPlayer player, string content)
    {
        _luaInteropService.SetClipboard(player, content);
    }

    private void _luaInteropService_ClientErrorMessage(Player player, string message, int level, string file, int line)
    {
        _logger.Warning("Clientside error on player {player} ({level}): {message} in {file}:{line}", (RPGPlayer)player, level, message, file, line);
    }
}
