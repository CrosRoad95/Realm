using RealmCore.Server.Components.Elements.Abstractions;

namespace RealmCore.Server.Logic.Resources;

internal sealed class ClientInterfaceResourceLogic
{
    private readonly ILogger<ClientInterfaceResourceLogic> _logger;
    private readonly IClientInterfaceService _clientInterfaceService;
    private readonly IEntityEngine _ecs;

    public ClientInterfaceResourceLogic(IClientInterfaceService clientInterfaceService, ILogger<ClientInterfaceResourceLogic> logger, IEntityEngine ecs)
    {
        _clientInterfaceService = clientInterfaceService;
        _ecs = ecs;
        _logger = logger;

        _clientInterfaceService.ClientErrorMessage += HandleClientErrorMessage;
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
            _clientInterfaceService.AddFocusable(component.Entity.GetElement());
            interactionComponent.Detached += HandleInteractionComponentDetachedFromEntity;
        }
    }

    private void HandleInteractionComponentDetachedFromEntity(Component component)
    {
        var interactionComponent = (InteractionComponent)component;
        interactionComponent.Detached -= HandleInteractionComponentDetachedFromEntity;
        _clientInterfaceService.RemoveFocusable(interactionComponent.Entity.GetElement());
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
