using RealmCore.Resources.Overlay;
using RealmCore.Server.Extensions.Resources;

namespace RealmCore.Console.Logic;

internal sealed class AchievementsLogic : ComponentLogic<AchievementsComponent>
{
    private readonly IOverlayService _overlayService;

    public AchievementsLogic(IEntityEngine ecs, IOverlayService overlayService) : base(ecs)
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
        _overlayService.AddNotification(achievementsComponent.Entity, $"Odblokowałeś osiągnięcie '{achievementName}'");
    }
}
