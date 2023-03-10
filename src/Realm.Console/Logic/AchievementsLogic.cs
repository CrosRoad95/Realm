namespace Realm.Console.Logic;

internal sealed class AchievementsLogic
{
    private readonly ECS _ecs;

    public AchievementsLogic(ECS ecs)
    {
        _ecs = ecs;

        _ecs.EntityCreated += HandleEntityCreated;
    }

    private void HandleEntityCreated(Entity entity)
    {
        if(entity.Tag == Entity.EntityTag.Player)
            entity.ComponentAdded += HandleComponentAdded;
    }

    private void HandleComponentAdded(Component component)
    {
        if (component is AchievementsComponent achievementsComponent)
        {
            achievementsComponent.AchievementUnlocked += HandleAchievementUnlocked;
        }
    }

    private void HandleAchievementUnlocked(AchievementsComponent achievementsComponent, string achievementName)
    {
        var playerElementComponent = achievementsComponent.Entity.GetRequiredComponent<PlayerElementComponent>();
        playerElementComponent.AddNotification($"Odblokowałeś osiągnięcie '{achievementName}'");
    }
}
