namespace Realm.Console.Logic;

internal sealed class PlayerJoinedLogic
{
    private readonly ECS _ecs;

    public PlayerJoinedLogic(ECS ecs)
    {
        _ecs = ecs;

        _ecs.EntityCreated += HandleEntityCreated;
    }

    private async void HandleEntityCreated(Entity entity)
    {
        if (entity.Tag != Entity.PlayerTag)
            return;

        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
        playerElementComponent.SetChatVisible(false);
        playerElementComponent.ClearChatBox();
        var adminComp = await entity.AddComponentAsync(new AdminComponent());
        adminComp.DebugView = true;
        entity.AddComponent(new LoginGuiComponent());

        entity.ComponentAdded += HandleComponentAdded;
    }

    private void HandleComponentAdded(Component component)
    {
        if (component is AccountComponent)
        {
            var playerElementComponent = component.Entity.GetRequiredComponent<PlayerElementComponent>();
            playerElementComponent.SetChatVisible(true);
            playerElementComponent.ClearChatBox();
            if (!playerElementComponent.TrySpawnAtLastPosition())
            {
                playerElementComponent.Spawn(new Vector3(362.58f + (float)Random.Shared.NextDouble() * 3, -91.07f + (float)Random.Shared.NextDouble() * 3, 1.38f),
                    new Vector3(0, 0, 90));
            }
        }
    }
}
