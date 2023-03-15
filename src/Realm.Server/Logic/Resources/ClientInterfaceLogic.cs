namespace Realm.Server.Logic.Resources;

internal class ClientInterfaceLogic
{
    private readonly ILogger<ClientInterfaceLogic> _logger;
    private readonly ClientInterfaceService _ClientInterfaceService;
    private readonly IEntityByElement _entityByElement;

    public ClientInterfaceLogic(ClientInterfaceService ClientInterfaceService, ILogger<ClientInterfaceLogic> logger, IEntityByElement entityByElement)
    {
        _ClientInterfaceService = ClientInterfaceService;
        _entityByElement = entityByElement;
        _logger = logger;

        _ClientInterfaceService.ClientErrorMessage += HandleClientErrorMessage;
        _ClientInterfaceService.FocusedElementChanged += HandleFocusedElementChanged;
    }

    private void HandleFocusedElementChanged(Player player, Element? focusedElement)
    {
        if (!_entityByElement.TryGetEntityByPlayer(player, out var playerEntity))
            return;

        if(playerEntity.TryGetComponent(out PlayerElementComponent playerElementComponent))
        {
            if (focusedElement == null)
                playerElementComponent.FocusedEntity = null;
            else
            {
                _entityByElement.TryGetByElement(focusedElement, out var entity);
                playerElementComponent.FocusedEntity = entity;
            }
        }
    }

    private void HandleClientErrorMessage(Player player, string message, int level, string file, int line)
    {
        if (!_entityByElement.TryGetEntityByPlayer(player, out var playerEntity))
            return;

        var playerName = playerEntity?.Name ?? player.Name;
        switch (level)
        {
            case 0: // Custom
            case 3: // Information
                if (line > 0)
                    _logger.LogInformation("Clientside: {player} ({level}): {message} in {file}:{line}", playerName, level, message, file, line);
                else
                    _logger.LogInformation("Clientside: {player} ({level}): {message}", playerName, level, message);
                break;
            case 2: // Warning
                if (line > 0)
                    _logger.LogWarning("Clientside: {player} ({level}): {message} in {file}:{line}", playerName, level, message, file, line);
                else
                    _logger.LogWarning("Clientside: {player} ({level}): {message}", playerName, level, message);
                break;
            default: // Error or something else
                if (line > 0)
                    _logger.LogError("Clientside: {player} ({level}): {message} in {file}:{line}", playerName, level, message, file, line);
                else
                    _logger.LogError("Clientside: {player} ({level}): {message}", playerName, level, message);
                break;
        }
    }
}
