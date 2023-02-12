using Realm.Resources.Assets;
using Realm.Resources.Assets.Interfaces;
using System.Drawing;

namespace Realm.Console.Logic;

internal sealed class PlayerJoinedLogic
{
    private readonly ECS _ecs;
    private readonly ILogger<PlayerJoinedLogic> _logger;
    private readonly AssetsRegistry _assetsRegistry;

    public PlayerJoinedLogic(ECS ecs, ILogger<PlayerJoinedLogic> logger, AssetsRegistry assetsRegistry)
    {
        _ecs = ecs;
        _logger = logger;
        _assetsRegistry = assetsRegistry;
        _ecs.EntityCreated += HandleEntityCreated;
    }

    private void HandleEntityCreated(Entity entity)
    {
        if (entity.Tag != Entity.EntityTag.Player)
            return;

        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
        playerElementComponent.SetRenderingEnabled(false);
        playerElementComponent.SetChatVisible(false);
        playerElementComponent.ClearChatBox();
        playerElementComponent.FadeCamera(CameraFade.In);
        playerElementComponent.SetCameraMatrix(new Vector3(379.89844f, -92.6416f, 10.950561f), new Vector3(336.75684f, -93.018555f, 1.3956465f));
        var adminComponent = entity.AddComponent<AdminComponent>();
        adminComponent.DebugView = true;
        adminComponent.DevelopmentMode = true;
        entity.AddComponent(new LoginGuiComponent());

        entity.ComponentAdded += HandleComponentAdded;
        entity.Destroyed += HandleDestroyed;
    }

    private void HandleDestroyed(Entity entity)
    {
        entity.ComponentAdded -= HandleComponentAdded;
    }

    private async void HandleComponentAdded(Component component)
    {
        try
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
                await Task.Delay(300);
                await playerElementComponent.FadeCameraAsync(CameraFade.In);
                playerElementComponent.SetRenderingEnabled(true);
                var hud = playerElementComponent.CreateHud("testhud", x => x
                    .AddRectangle(new Vector2(x.Right - 400, 600), new Size(400, 20), Color.DarkBlue)
                    .AddText("foo bar", new Vector2(x.Right - 200, 600), new Size(200, 20), font: "default", alignX: "center", alignY: "center")
                    .AddText("custom font", new Vector2(x.Right - 400, 600), new Size(200, 20), font: _assetsRegistry.GetAsset<IFont>("Better Together.otf"), alignX: "center", alignY: "center"));
                hud.SetVisible(true);
            }

            if (component is LevelComponent levelComponent)
            {
                levelComponent.LevelChanged += (self, level) =>
                {
                    _logger.LogInformation("Player leveled up: {level}", level);
                };
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to add component.");
        }
    }
}
