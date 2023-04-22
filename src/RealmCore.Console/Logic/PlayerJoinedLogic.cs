using RealmCore.Server.Components.Peds;
using RealmCore.Server.Components;
using RealmCore.Server.Enums;
using RealmCore.Server.Extensions;
using RealmCore.Resources.Nametags;
using RealmCore.Resources.Admin.Enums;

namespace RealmCore.Console.Logic;

internal sealed class PlayerJoinedLogic
{
    private readonly IECS _ecs;
    private readonly ILogger<PlayerJoinedLogic> _logger;
    private readonly INametagsService _nametagsService;

    public PlayerJoinedLogic(IECS ecs, ILogger<PlayerJoinedLogic> logger, INametagsService nametagsService)
    {
        _ecs = ecs;
        _logger = logger;
        _nametagsService = nametagsService;
        _ecs.EntityCreated += HandleEntityCreated;
    }

    private void HandleEntityCreated(Entity entity)
    {
        if (entity.Tag != EntityTag.Player)
            return;

        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();

        playerElementComponent.SetText3dRenderingEnabled(false);
        playerElementComponent.SetChatVisible(false);
        playerElementComponent.ClearChatBox();
        playerElementComponent.FadeCamera(CameraFade.In);
        playerElementComponent.SetCameraMatrix(new Vector3(379.89844f, -92.6416f, 10.950561f), new Vector3(336.75684f, -93.018555f, 1.3956465f));
        var adminComponent = entity.AddComponent(new AdminComponent(new List<AdminTool> { AdminTool.Entities, AdminTool.Components }));
        adminComponent.DebugView = true;
        adminComponent.DevelopmentMode = true;
        entity.AddComponent<LoginGuiComponent>();

        entity.ComponentAdded += HandleComponentAdded;
        entity.Disposed += HandleDisposed;
    }

    private void HandleDisposed(Entity entity)
    {
        entity.ComponentAdded -= HandleComponentAdded;
    }

    private async void HandleComponentAdded(Component component)
    {
        try
        {
            if (component is UserComponent)
            {
                var entity = component.Entity;
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
                playerElementComponent.SetText3dRenderingEnabled(true);
                entity.AddComponent(new NametagComponent("KoxKociarz"));
                _nametagsService.SetNametagRenderingEnabled(component.Entity, true);
            }

            if (component is LevelComponent levelComponent)
            {
                levelComponent.LevelChanged += (self, level) =>
                {
                    _logger.LogInformation("Player leveled up: {level}", level);
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add component.");
        }
    }
}
