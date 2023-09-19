namespace RealmCore.Server.Logic.Components;

internal sealed class FocusableComponentLogic : ComponentLogic<FocusableComponent>
{
    private readonly IEntityEngine _entityEngine;
    private readonly IClientInterfaceService _clientInterfaceService;

    public FocusableComponentLogic(IEntityEngine entityEngine, IClientInterfaceService clientInterfaceService) : base(entityEngine)
    {
        _entityEngine = entityEngine;
        _clientInterfaceService = clientInterfaceService;
        _clientInterfaceService.FocusedElementChanged += HandleFocusedElementChanged;
    }

    private void HandleFocusedElementChanged(Player player, Element? focusedElement)
    {
        if (!_entityEngine.TryGetEntityByPlayer(player, out var playerEntity))
            return;

        var playerElementComponent = playerEntity.GetRequiredComponent<PlayerElementComponent>();
        if(focusedElement == null)
        {
            if(playerElementComponent.FocusedEntity != null)
            {
                if(playerElementComponent.FocusedEntity.TryGetComponent(out FocusableComponent focusableComponent))
                {
                    focusableComponent.RemoveFocusedPlayer(playerEntity);
                }
            }
        }
        else
        {
            if(_entityEngine.TryGetByElement(focusedElement, out var elementEntity))
            {
                elementEntity.GetRequiredComponent<FocusableComponent>().AddFocusedPlayer(playerEntity);
            }
        }

        if (focusedElement == null)
            playerElementComponent.FocusedEntity = null;
        else
        {
            _entityEngine.TryGetByElement(focusedElement, out var entity);
            playerElementComponent.FocusedEntity = entity;
        }

    }

    protected override void ComponentAdded(FocusableComponent component)
    {
        _clientInterfaceService.AddFocusable(component.Entity.GetElement());
    }

    protected override void ComponentDetached(FocusableComponent component)
    {
        _clientInterfaceService.RemoveFocusable(component.Entity.GetElement());
    }
}
