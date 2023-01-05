using SlipeServer.Packets.Lua.Camera;

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
        await playerElementComponent.FadeCameraAsync(CameraFade.In);
        playerElementComponent.SetCameraMatrix(new Vector3(379.89844f, -92.6416f, 10.950561f), new Vector3(336.75684f, -93.018555f, 1.3956465f));
        var adminComp = await entity.AddComponentAsync(new AdminComponent());
        adminComp.DebugView = true;
        entity.AddComponent(new LoginGuiComponent());

        entity.ComponentAdded += HandleComponentAdded;
    }

    private async void HandleComponentAdded(Component component)
    {
        if (component is AccountComponent)
        {
            var playerElementComponent = component.Entity.GetRequiredComponent<PlayerElementComponent>();
            await playerElementComponent.FadeCameraAsync(CameraFade.Out);
            playerElementComponent.SetChatVisible(true);
            playerElementComponent.SetGuiDebugToolsEnabled(true);
            playerElementComponent.ClearChatBox();
            playerElementComponent.SetCameraTarget(component.Entity);
            if (!playerElementComponent.TrySpawnAtLastPosition())
            {
                playerElementComponent.Spawn(new Vector3(362.58f + (float)Random.Shared.NextDouble() * 3, -91.07f + (float)Random.Shared.NextDouble() * 3, 1.38f),
                    new Vector3(0, 0, 90));
            }
            await playerElementComponent.FadeCameraAsync(CameraFade.In);
        }
    }
}
