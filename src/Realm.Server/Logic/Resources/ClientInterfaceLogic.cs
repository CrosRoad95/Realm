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
        Entity? entity = _entityByElement.TryGetByElement(player);
        if (entity == null)
            return;

        if(entity.TryGetComponent(out PlayerElementComponent playerElementComponent))
        {
            if (focusedElement == null)
                playerElementComponent.FocusedEntity = null;
            else
                playerElementComponent.FocusedEntity = _entityByElement.TryGetByElement(focusedElement);
        }
    }

    private void HandleClientErrorMessage(Player player, string message, int level, string file, int line)
    {
        Entity? entity = _entityByElement.TryGetByElement(player);
        var playerName = entity?.Name ?? player.Name;
        switch (level)
        {
            case 0: // Custom
            case 3: // Information
                _logger.LogInformation("Clientside: {player} ({level}): {message} in {file}:{line}", playerName, level, message, file, line);
                break;
            case 2: // Warning
                _logger.LogWarning("Clientside: {player} ({level}): {message} in {file}:{line}", playerName, level, message, file, line);
                break;
            default: // Error or something else
                _logger.LogError("Clientside: {player} ({level}): {message} in {file}:{line}", playerName, level, message, file, line);
                break;
        }
    }
}
