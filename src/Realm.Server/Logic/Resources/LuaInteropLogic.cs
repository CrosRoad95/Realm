using Realm.Domain.Interfaces;

namespace Realm.Server.Logic.Resources;

internal class ClientInterfaceLogic
{
    private readonly ILogger _logger;
    private readonly ClientInterfaceService _ClientInterfaceService;
    private readonly IEntityByElement _entityByElement;

    public ClientInterfaceLogic(ClientInterfaceService ClientInterfaceService, ILogger logger, IEntityByElement entityByElement)
    {
        _ClientInterfaceService = ClientInterfaceService;
        _entityByElement = entityByElement;
        _logger = logger.ForContext<ClientInterfaceLogic>();

        _ClientInterfaceService.ClientErrorMessage += HandleClientErrorMessage;
    }

    private void HandleClientErrorMessage(Player player, string message, int level, string file, int line)
    {
        Entity? entity = _entityByElement.TryGetByElement(player);
        var playerName = entity?.Name ?? player.Name;
        switch (level)
        {
            case 0: // Custom
            case 3: // Information
                _logger.Information("Clientside: {player} ({level}): {message} in {file}:{line}", playerName, level, message, file, line);
                break;
            case 2: // Warning
                _logger.Warning("Clientside: {player} ({level}): {message} in {file}:{line}", playerName, level, message, file, line);
                break;
            default: // Error or something else
                _logger.Error("Clientside: {player} ({level}): {message} in {file}:{line}", playerName, level, message, file, line);
                break;
        }
    }
}
