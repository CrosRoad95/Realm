namespace RealmCore.Server.Modules.World;

internal sealed class FocusableElementsLogic : PlayerLifecycle
{
    private readonly IClientInterfaceService _clientInterfaceService;

    public FocusableElementsLogic(PlayersEventManager playersEventManager, IElementFactory elementFactory, IClientInterfaceService clientInterfaceService) : base(playersEventManager)
    {
        _clientInterfaceService = clientInterfaceService;
        _clientInterfaceService.FocusedElementChanged += HandleFocusedElementChanged;
        _clientInterfaceService.ClickedElementChanged += HandleClickedElementChanged;
        elementFactory.ElementCreated += HandleElementCreated;
    }

    private void HandleElementCreated(IElementFactory elementFactory, Element element)
    {
        if (element is IFocusableElement)
        {
            _clientInterfaceService.AddFocusable(element);
            element.Destroyed += HandleDestroyed;
        }
    }
    
    private void HandleScopedElementCreated(IElementFactory elementFactory, Element element)
    {
        if (element is not IFocusableElement)
            return;

        var scopedElementFactory = (IScopedElementFactory)elementFactory;
        var player = scopedElementFactory.Player;
        _clientInterfaceService.AddFocusableFor(element, player);

        void handleScopedDestroyed(Element element)
        {
            element.Destroyed -= handleScopedDestroyed;
            _clientInterfaceService.RemoveFocusableFor(element, player);
        }

        element.Destroyed += handleScopedDestroyed;
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.ElementFactory.ElementCreated += HandleScopedElementCreated;
    }

    protected override void PlayerLeft(RealmPlayer player)
    {
        player.ElementFactory.ElementCreated -= HandleScopedElementCreated;
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
        if (focusedElement is IFocusableElement || focusedElement == null)
        {
            var player = (RealmPlayer)plr;
            player.FocusedElement = focusedElement;
            player.FocusedVehiclePart = vehiclePart;
        }
    }
}
