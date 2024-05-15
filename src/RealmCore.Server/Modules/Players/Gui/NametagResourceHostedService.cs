
namespace RealmCore.Server.Modules.Players.Gui;

internal sealed class NametagResourceHostedService : PlayerLifecycle, IHostedService
{
    private readonly INametagsService _nametagsService;

    public NametagResourceHostedService(PlayersEventManager playersEventManager, INametagsService nametagsService) : base(playersEventManager)
    {
        _nametagsService = nametagsService;
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
        player.NametagTextChanged += HandleNametagTextChanged;
    }

    private void HandleNametagTextChanged(RealmPlayer player, string? nametagText)
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
