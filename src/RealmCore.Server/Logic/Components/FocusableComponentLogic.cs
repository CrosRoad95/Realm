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
        _clientInterfaceService.ClickedElementChanged += HandleClickedElementChanged;
    }

    private void HandleClickedElementChanged(Player player, Element? clickedElement)
    {
        if (!_entityEngine.TryGetEntityByPlayer(player, out var playerEntity) || playerEntity == null)
            return;

        var playerElementComponent = playerEntity.GetRequiredComponent<PlayerElementComponent>();
        if(clickedElement != null && _entityEngine.TryGetByElement(clickedElement, out var clickedElementEntity))
        {
            playerElementComponent.LastClickedElement = clickedElementEntity;
        }
        else
        {
            playerElementComponent.LastClickedElement = null;
        }
    }

    private void HandleFocusedElementChanged(Player player, Element? focusedElement, string? vehiclePart)
    {
        if (!_entityEngine.TryGetEntityByPlayer(player, out var playerEntity) || playerEntity == null)
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
            if(_entityEngine.TryGetByElement(focusedElement, out var elementEntity) && elementEntity != null)
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
