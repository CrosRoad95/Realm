using RealmCore.Sample.Components.Gui;

namespace RealmCore.Sample.Logic;

internal sealed class PlayerJoinedLogic
{
    private readonly IEntityEngine _entityEngine;
    private readonly ILogger<PlayerJoinedLogic> _logger;
    private readonly ILogger<LoginGuiComponent> _loggerLoginGuiComponent;
    private readonly ILogger<RegisterGuiComponent> _loggerRegisterGuiComponent;
    private readonly INametagsService _nametagsService;
    private readonly IUsersService _usersService;
    private readonly ChatBox _chatBox;
    private readonly Text3dService _text3DService;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<UserData> _userManager;
    private readonly IGuiSystemService? _guiSystemService;

    public PlayerJoinedLogic(IEntityEngine ecs, ILogger<PlayerJoinedLogic> logger, ILogger<LoginGuiComponent> loggerLoginGuiComponent, ILogger<RegisterGuiComponent> loggerRegisterGuiComponent, INametagsService nametagsService, IUsersService usersService, ChatBox chatBox, Text3dService text3DService, IUserRepository userRepository, UserManager<UserData> userManager, IGuiSystemService? guiSystemService = null)
    {
        _entityEngine = ecs;
        _logger = logger;
        _loggerLoginGuiComponent = loggerLoginGuiComponent;
        _loggerRegisterGuiComponent = loggerRegisterGuiComponent;
        _nametagsService = nametagsService;
        _usersService = usersService;
        _chatBox = chatBox;
        _text3DService = text3DService;
        _userRepository = userRepository;
        _userManager = userManager;
        _guiSystemService = guiSystemService;
        _entityEngine.EntityCreated += HandleEntityCreated;
    }

    private void HandleEntityCreated(Entity entity)
    {
        if (!entity.HasComponent<PlayerTagComponent>())
            return;

        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();

        _text3DService.SetRenderingEnabled(entity, false);
        _chatBox.SetVisibleFor(entity, false);
        _chatBox.ClearFor(entity);
        playerElementComponent.FadeCamera(CameraFade.In);
        playerElementComponent.SetCameraMatrix(new Vector3(379.89844f, -92.6416f, 10.950561f), new Vector3(336.75684f, -93.018555f, 1.3956465f));
        var adminComponent = entity.AddComponent(new AdminComponent(new List<AdminTool> { AdminTool.Entities, AdminTool.Components, AdminTool.ShowSpawnMarkers }));
        adminComponent.DebugView = true;
        adminComponent.DevelopmentMode = true;
        entity.AddComponent(new LoginGuiComponent(_usersService, _loggerLoginGuiComponent, _loggerRegisterGuiComponent, _userRepository, _userManager));

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
                if (_guiSystemService != null)
                    _guiSystemService.SetDebugToolsEnabled(RealmInternal.GetPlayer(playerElementComponent), true);
                _chatBox.SetVisibleFor(entity, true);
                _chatBox.ClearFor(entity);
                playerElementComponent.SetCameraTarget(component.Entity);
                if (!playerElementComponent.TrySpawnAtLastPosition())
                {
                    playerElementComponent.Spawn(new Vector3(362.58f + (float)Random.Shared.NextDouble() * 3, -91.07f + (float)Random.Shared.NextDouble() * 3, 1.38f),
                        new Vector3(0, 0, 90));
                }
                await Task.Delay(300);
                await playerElementComponent.FadeCameraAsync(CameraFade.In);
                _text3DService.SetRenderingEnabled(entity, true);
                entity.AddComponent(new NametagComponent("KoxKociarz"));
                _nametagsService.SetNametagRenderingEnabled(component.Entity, true);
            }

            if (component is LevelComponent levelComponent)
            {
                levelComponent.LevelChanged += (self, level, up) =>
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
