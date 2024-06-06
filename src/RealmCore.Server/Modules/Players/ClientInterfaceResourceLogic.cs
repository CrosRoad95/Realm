namespace RealmCore.Server.Modules.Players;

internal sealed class ClientInterfaceResourceLogic : IHostedService
{
    private readonly IClientInterfaceService _clientInterfaceService;
    private readonly IElementFactory _elementFactory;

    public ClientInterfaceResourceLogic(IClientInterfaceService clientInterfaceService, IElementFactory elementFactory)
    {
        _clientInterfaceService = clientInterfaceService;
        _elementFactory = elementFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _elementFactory.ElementCreated += HandleElementCreated;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _elementFactory.ElementCreated -= HandleElementCreated;
        return Task.CompletedTask;
    }

    private void HandleElementCreated(IElementFactory elementFactory, Element element)
    {
        if (element is IFocusableElement focusableElement)
        {
            _clientInterfaceService.AddFocusable(element);
            element.Destroyed += HandleElementDestroyed;
        }
    }

    private void HandleElementDestroyed(Element element)
    {
        _clientInterfaceService.RemoveFocusable(element);
        element.Destroyed -= HandleElementDestroyed;
    }
}
