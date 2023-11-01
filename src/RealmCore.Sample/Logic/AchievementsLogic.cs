using RealmCore.Resources.Overlay;

namespace RealmCore.Sample.Logic;

internal sealed class AchievementsLogic : ComponentLogic<AchievementsComponent>
{
    private readonly IOverlayService _overlayService;

    public AchievementsLogic(IElementFactory elementFactory, IOverlayService overlayService) : base(elementFactory)
    {
        _overlayService = overlayService;
    }

    protected override void ComponentAdded(AchievementsComponent achievementsComponent)
    {
        achievementsComponent.AchievementUnlocked += HandleAchievementUnlocked;
    }

    protected override void ComponentDetached(AchievementsComponent achievementsComponent)
    {
        achievementsComponent.AchievementUnlocked -= HandleAchievementUnlocked;
    }

    private void HandleAchievementUnlocked(AchievementsComponent achievementsComponent, int achievementName)
    {
        _overlayService.AddNotification((RealmPlayer)achievementsComponent.Element, $"Odblokowałeś osiągnięcie '{achievementName}'");
    }
}
