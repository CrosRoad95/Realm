namespace RealmCore.Server.Modules.Players.Gui;

internal sealed class NametagResourceHostedService : PlayerLifecycle, IHostedService
{
    private readonly INametagsService _nametagsService;

    public NametagResourceHostedService(PlayersEventManager playersEventManager, INametagsService nametagsService, IElementFactory elementFactory) : base(playersEventManager)
    {
        _nametagsService = nametagsService;
        elementFactory.ElementCreated += HandleElementCreated;
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.Nametag.TextChanged += HandleTextChanged;
        player.Nametag.ShowMyNametagChanged += HandleShowMyNametagChanged;
    }

    private void HandleShowMyNametagChanged(Nametag nametag, bool enabled)
    {
        if (nametag.Ped is not Player player)
            return;

        _nametagsService.SetLocalPlayerRenderingEnabled(player, enabled);
    }

    private void HandleTextChanged(Nametag nametag, string? text)
    {
        if(text == null)
        {
            _nametagsService.RemoveNametag(nametag.Ped);
        }
        else
        {
            _nametagsService.SetNametag(nametag.Ped, text);
        }
    }

    private void HandleElementCreated(IElementFactory elementFactory, Element element)
    {
        if (element is not RealmPed ped)
            return;

        ped.Nametag.TextChanged += HandleTextChanged;
        if (ped.Nametag.Text != null)
            _nametagsService.SetNametag(ped.Nametag.Ped, ped.Nametag.Text);
    }

    private void HandlePlayerNametagTextChanged(RealmPlayer player, string? nametagText)
    {
        if (nametagText != null)
        {
            _nametagsService.SetNametag(player, nametagText);
        }
        else
        {
            _nametagsService.RemoveNametag(player);
        }
    }
}
