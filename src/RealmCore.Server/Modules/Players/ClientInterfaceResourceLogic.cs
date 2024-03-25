namespace RealmCore.Server.Modules.Players;

internal sealed class ClientInterfaceResourceLogic
{
    private readonly ILogger<ClientInterfaceResourceLogic> _logger;
    private readonly IClientInterfaceService _clientInterfaceService;
    private readonly IElementFactory _elementFactory;

    public ClientInterfaceResourceLogic(IClientInterfaceService clientInterfaceService, ILogger<ClientInterfaceResourceLogic> logger, IElementFactory elementFactory)
    {
        _clientInterfaceService = clientInterfaceService;
        _elementFactory = elementFactory;
        _logger = logger;

        _clientInterfaceService.ClientErrorMessage += HandleClientErrorMessage;
        _elementFactory.ElementCreated += HandleElementCreated;
    }

    private void HandleElementCreated(IElementFactory elementFactory, Element element)
    {
        if (element is IFocusableElement focusableElement)
        {
            _clientInterfaceService.AddFocusable(element);
            element.Destroyed += HandleElementDestroyed;
        }
    }

    private void HandleElementDestroyed(Element element)
    {
        _clientInterfaceService.RemoveFocusable(element);
        element.Destroyed -= HandleElementDestroyed;
    }

    private void HandleClientErrorMessage(Player player, string message, int level, string file, int line)
    {
        var playerName = player.Name;
        switch (level)
        {
            case 0: // Custom
            case 3: // Information
                if (line > 0)
                    _logger.LogTrace("Clientside: {player} ({level}): {message} in {file}:{line}", playerName, level, message, file, line);
                else
                    _logger.LogTrace("Clientside: {player} ({level}): {message}", playerName, level, message);
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
