namespace RealmCore.BlazorGui.Logic;

internal sealed class AchievementsHostedService : PlayerLifecycle, IHostedService
{
    private readonly IOverlayService _overlayService;

    public AchievementsHostedService(PlayersEventManager playersEventManager, IOverlayService overlayService) : base(playersEventManager)
    {
        _overlayService = overlayService;
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
        player.Achievements.Unlocked += HandleUnlocked;
    }

    protected override void PlayerLeft(RealmPlayer player)
    {
        player.Achievements.Unlocked -= HandleUnlocked;
    }

    private void HandleUnlocked(IPlayerAchievementsFeature achievementService, int achievementId)
    {
        _overlayService.AddNotification(achievementService.Player, $"Odblokowałeś osiągnięcie '{achievementId}'");
    }
}
