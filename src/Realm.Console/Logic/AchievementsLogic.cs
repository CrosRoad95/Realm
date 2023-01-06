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
        if(entity.Tag == Entity.PlayerTag)
            entity.ComponentAdded += HandleComponentAdded;
    }

    private void HandleComponentAdded(Component component)
    {
        if (component is AchievementsComponent achievementsComponent)
        {
            achievementsComponent.AchievementUnlocked += HandleAchievementUnlocked;
        }
    }

    private void HandleAchievementUnlocked(Entity entity, string achievementName)
    {
        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
        playerElementComponent.AddNotification($"Odblokowałeś osiągnięcie '{achievementName}'");
    }
}
