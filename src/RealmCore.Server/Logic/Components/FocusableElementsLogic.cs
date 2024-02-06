namespace RealmCore.Server.Logic.Components;

internal sealed class FocusableElementsLogic
{
    private readonly IClientInterfaceService _clientInterfaceService;

    public FocusableElementsLogic(IElementFactory elementFactory, IClientInterfaceService clientInterfaceService)
    {
        _clientInterfaceService = clientInterfaceService;
        _clientInterfaceService.FocusedElementChanged += HandleFocusedElementChanged;
        _clientInterfaceService.ClickedElementChanged += HandleClickedElementChanged;
        elementFactory.ElementCreated += HandleElementCreated;
    }

    private void HandleElementCreated(Element element)
    {
        if(element is IFocusableElement)
        {
            _clientInterfaceService.AddFocusable(element);
            element.Destroyed += HandleDestroyed;
        }
    }

    private void HandleDestroyed(Element element)
    {
        element.Destroyed -= HandleDestroyed;
        _clientInterfaceService.RemoveFocusable(element);
    }

    private void HandleClickedElementChanged(Player plr, Element? clickedElement)
    {
        ((RealmPlayer)plr).LastClickedElement = clickedElement;
    }

    private void HandleFocusedElementChanged(Player plr, Element? focusedElement, string? vehiclePart)
    {
        if(focusedElement is IFocusableElement)
        {
            ((RealmPlayer)plr).FocusedElement = focusedElement;
        }
    }
}
