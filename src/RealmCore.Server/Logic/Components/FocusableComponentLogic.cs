namespace RealmCore.Server.Logic.Components;

internal sealed class FocusableComponentLogic : ComponentLogic<FocusableComponent>
{
    private readonly IClientInterfaceService _clientInterfaceService;

    public FocusableComponentLogic(IElementFactory elementFactory, IClientInterfaceService clientInterfaceService) : base(elementFactory)
    {
        _clientInterfaceService = clientInterfaceService;
        _clientInterfaceService.FocusedElementChanged += HandleFocusedElementChanged;
        _clientInterfaceService.ClickedElementChanged += HandleClickedElementChanged;
    }

    private void HandleClickedElementChanged(Player player, Element? clickedElement)
    {
        ((RealmPlayer)player).LastClickedElement = clickedElement;
    }

    // TODO:
    private void HandleFocusedElementChanged(Player player, Element? focusedElement, string? vehiclePart)
    {
        var realmPlayer = ((RealmPlayer)player);
        if(focusedElement == null)
        {
            if(realmPlayer.FocusedEntity is IComponents components)
            {
                if(components.Components.TryGetComponent(out FocusableComponent focusableComponent))
                {
                    focusableComponent.RemoveFocusedPlayer(realmPlayer);
                }
            }
        }
        else
        {
            if (realmPlayer.FocusedEntity is IComponents components)
            {
                components.Components.GetRequiredComponent<FocusableComponent>().AddFocusedPlayer(realmPlayer);

            }
        }

        realmPlayer.FocusedEntity = focusedElement;
    }

    protected override void ComponentAdded(FocusableComponent component)
    {
        _clientInterfaceService.AddFocusable(component.Element);
    }

    protected override void ComponentDetached(FocusableComponent component)
    {
        _clientInterfaceService.RemoveFocusable(component.Element);
    }
}
