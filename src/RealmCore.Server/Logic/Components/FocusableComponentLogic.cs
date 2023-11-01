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
    private void HandleFocusedElementChanged(Player plr, Element? focusedElement, string? vehiclePart)
    {
        var player = ((RealmPlayer)plr);
        if(focusedElement == null)
        {
            if(player.FocusedElement is IComponents components)
            {
                if(components.Components.TryGetComponent(out FocusableComponent focusableComponent))
                {
                    focusableComponent.RemoveFocusedPlayer(player);
                }
            }
        }
        else
        {
            if (player.FocusedElement is IComponents components)
            {
                components.Components.GetRequiredComponent<FocusableComponent>().AddFocusedPlayer(player);

            }
        }

        player.FocusedElement = focusedElement;
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
