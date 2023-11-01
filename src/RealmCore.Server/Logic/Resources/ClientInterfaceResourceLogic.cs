using RealmCore.Server.Components.Abstractions;

namespace RealmCore.Server.Logic.Resources;

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

    private void HandleElementCreated(Element element)
    {
        if(element is IComponents components)
        {
            element.Destroyed += HandleElementDestroyed;
            components.Components.ComponentAdded += HandleComponentAdded;
        }
    }

    private void HandleElementDestroyed(Element element)
    {
        if (element is IComponents components)
        {
            components.Components.ComponentAdded -= HandleComponentAdded;
        }
    }

    private void HandleComponentAdded(IComponent component)
    {
        if (component is InteractionComponent interactionComponent)
        {
            _clientInterfaceService.AddFocusable(component.Element);
            interactionComponent.Detached += HandleInteractionComponentDetachedFromElement;
        }
    }

    private void HandleInteractionComponentDetachedFromElement(IComponentLifecycle component)
    {
        var interactionComponent = (InteractionComponent)component;
        interactionComponent.Detached -= HandleInteractionComponentDetachedFromElement;
        _clientInterfaceService.RemoveFocusable(interactionComponent.Element);
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
