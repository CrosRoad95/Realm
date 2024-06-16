namespace RealmCore.Server.Modules.World;

internal sealed class FocusableElementsHostedService : PlayerLifecycle, IHostedLifecycleService
{
    private readonly IElementFactory _elementFactory;
    private readonly IClientInterfaceService _clientInterfaceService;

    public FocusableElementsHostedService(PlayersEventManager playersEventManager, IElementFactory elementFactory, IClientInterfaceService clientInterfaceService) : base(playersEventManager)
    {
        _elementFactory = elementFactory;
        _clientInterfaceService = clientInterfaceService;
    }

    private void HandleElementCreated(IElementFactory elementFactory, Element element)
    {
        if (element is not IFocusableElement)
            return;

        _clientInterfaceService.AddFocusable(element);
        element.Destroyed += HandleDestroyed;
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

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        _clientInterfaceService.FocusedElementChanged += HandleFocusedElementChanged;
        _clientInterfaceService.ClickedElementChanged += HandleClickedElementChanged;
        _elementFactory.ElementCreated += HandleElementCreated;
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        _clientInterfaceService.FocusedElementChanged -= HandleFocusedElementChanged;
        _clientInterfaceService.ClickedElementChanged -= HandleClickedElementChanged;
        _elementFactory.ElementCreated -= HandleElementCreated;
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
