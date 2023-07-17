using RealmCore.Server.Abstractions;
namespace RealmCore.Console.Logic;

internal sealed class AchievementsLogic : ComponentLogic<AchievementsComponent>
{
    public AchievementsLogic(IECS ecs) : base(ecs) { }

    protected override void ComponentAdded(AchievementsComponent achievementsComponent)
    {
        achievementsComponent.AchievementUnlocked += HandleAchievementUnlocked;
    }

    protected override void ComponentRemoved(AchievementsComponent achievementsComponent)
    {
        achievementsComponent.AchievementUnlocked += HandleAchievementUnlocked;
    }

    private void HandleAchievementUnlocked(AchievementsComponent achievementsComponent, int achievementName)
    {
        var playerElementComponent = achievementsComponent.Entity.GetRequiredComponent<PlayerElementComponent>();
        playerElementComponent.AddNotification($"Odblokowałeś osiągnięcie '{achievementName}'");
    }
}
