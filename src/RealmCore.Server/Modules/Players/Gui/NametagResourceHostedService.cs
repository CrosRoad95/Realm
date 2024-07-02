
using SlipeServer.Server.Elements;

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
        player.NametagTextChanged += HandlePlayerNametagTextChanged;
        if(player.NametagText != null)
            _nametagsService.SetNametag(player, player.NametagText);
    }

    private void HandleElementCreated(IElementFactory elementFactory, Element element)
    {
        if (element is not RealmPed ped)
            return;

        ped.NametagTextChanged += HandlePedNametagTextChanged;
        if (ped.NametagText != null)
            _nametagsService.SetNametag(ped, ped.NametagText);
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

    private void HandlePedNametagTextChanged(RealmPed ped, string? nametagText)
    {
        if (nametagText != null)
        {
            _nametagsService.SetNametag(ped, nametagText);
        }
        else
        {
            _nametagsService.RemoveNametag(ped);
        }
    }
}
