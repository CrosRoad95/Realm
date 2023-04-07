namespace RealmCore.Server.Logic.Resources;

internal class ClientInterfaceLogic
{
    private readonly ILogger<ClientInterfaceLogic> _logger;
    private readonly IClientInterfaceService _clientInterfaceService;
    private readonly IECS _ecs;

    public ClientInterfaceLogic(IClientInterfaceService clientInterfaceService, ILogger<ClientInterfaceLogic> logger, IECS ecs)
    {
        _clientInterfaceService = clientInterfaceService;
        _ecs = ecs;
        _logger = logger;

        _clientInterfaceService.ClientErrorMessage += HandleClientErrorMessage;
        _clientInterfaceService.FocusedElementChanged += HandleFocusedElementChanged;
        _ecs.EntityCreated += HandleEntityCreated;
    }


    private void HandleEntityCreated(Entity entity)
    {
        entity.Disposed += HandleEntityDestroyed;
        entity.ComponentAdded += HandleComponentAdded;
    }

    private void HandleEntityDestroyed(Entity entity)
    {
        entity.ComponentAdded -= HandleComponentAdded;
    }

    private void HandleComponentAdded(Component component)
    {
        if (component is InteractionComponent interactionComponent)
        {
            _clientInterfaceService.AddFocusable(component.Entity.Element);
            interactionComponent.Disposed += HandleInteractionComponentDisposed;
        }
        if (component is ElementComponent elementComponent)
        {
            elementComponent.AddFocusableHandler = _clientInterfaceService.AddFocusable;
            elementComponent.RemoveFocusableHandler = _clientInterfaceService.RemoveFocusable;
        }
    }

    private void HandleInteractionComponentDisposed(Component component)
    {
        var interactionComponent = (InteractionComponent)component;
        interactionComponent.Disposed -= HandleInteractionComponentDisposed;
        _clientInterfaceService.RemoveFocusable(interactionComponent.Entity.Element);
    }

    private void HandleFocusedElementChanged(Player player, Element? focusedElement)
    {
        if (!_ecs.TryGetEntityByPlayer(player, out var playerEntity))
            return;

        if (playerEntity.TryGetComponent(out PlayerElementComponent playerElementComponent))
        {
            if (focusedElement == null)
                playerElementComponent.FocusedEntity = null;
            else
            {
                _ecs.TryGetByElement(focusedElement, out var entity);
                playerElementComponent.FocusedEntity = entity;
            }
        }
    }

    private void HandleClientErrorMessage(Player player, string message, int level, string file, int line)
    {
        if (!_ecs.TryGetEntityByPlayer(player, out var playerEntity))
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
