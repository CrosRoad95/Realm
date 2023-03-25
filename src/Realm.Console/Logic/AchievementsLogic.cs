using Realm.Domain.Components;
using Realm.Domain.Enums;

namespace Realm.Console.Logic;

internal sealed class AchievementsLogic
{
    private readonly IECS _ecs;

    public AchievementsLogic(IECS ecs)
    {
        _ecs = ecs;

        _ecs.EntityCreated += HandleEntityCreated;
    }

    private void HandleEntityCreated(Entity entity)
    {
        if(entity.Tag == EntityTag.Player)
            entity.ComponentAdded += HandleComponentAdded;
    }

    private void HandleComponentAdded(Component component)
    {
        if (component is AchievementsComponent achievementsComponent)
        {
            achievementsComponent.AchievementUnlocked += HandleAchievementUnlocked;
        }
    }

    private void HandleAchievementUnlocked(AchievementsComponent achievementsComponent, int achievementName)
    {
        var playerElementComponent = achievementsComponent.Entity.GetRequiredComponent<PlayerElementComponent>();
        playerElementComponent.AddNotification($"Odblokowałeś osiągnięcie '{achievementName}'");
    }
}
