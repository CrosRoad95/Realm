namespace RealmCore.Server.Logic.Components;

internal sealed class FocusableComponentLogic : ComponentLogic<FocusableComponent>
{
    private readonly IClientInterfaceService _clientInterfaceService;

    public FocusableComponentLogic(IEntityEngine entityEngine, IClientInterfaceService clientInterfaceService) : base(entityEngine)
    {
        _clientInterfaceService = clientInterfaceService;
        _clientInterfaceService.FocusedElementChanged += HandleFocusedElementChanged;
        _clientInterfaceService.ClickedElementChanged += HandleClickedElementChanged;
    }

    private void HandleClickedElementChanged(Player player, Element? clickedElement)
    {
        var playerElementComponent = (PlayerElementComponent)player;
        if (clickedElement is IElementComponent elementComponent)
        {
            playerElementComponent.LastClickedElement = elementComponent.Entity;
        }
        else
        {
            playerElementComponent.LastClickedElement = null;
        }
    }

    private void HandleFocusedElementChanged(Player player, Element? focusedElement, string? vehiclePart)
    {
        var playerEntity = ((PlayerElementComponent)player).Entity;
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
            if (focusedElement is IElementComponent elementComponent)
            {
                elementComponent.Entity.GetRequiredComponent<FocusableComponent>().AddFocusedPlayer(playerEntity);
            }
        }

        if (focusedElement == null)
            playerElementComponent.FocusedEntity = null;
        else
        {
            if (focusedElement is IElementComponent elementComponent)
                playerElementComponent.FocusedEntity = elementComponent.Entity;
            else
                playerElementComponent.FocusedEntity = null;
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
