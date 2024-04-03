namespace RealmCore.Sample.Logic;

internal sealed class AchievementsLogic : PlayerLifecycle
{
    private readonly IOverlayService _overlayService;

    public AchievementsLogic(PlayersEventManager playersEventManager, IOverlayService overlayService) : base(playersEventManager)
    {
        _overlayService = overlayService;
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
