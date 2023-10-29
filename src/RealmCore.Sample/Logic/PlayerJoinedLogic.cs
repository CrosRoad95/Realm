using RealmCore.Sample.Components.Gui;

namespace RealmCore.Sample.Logic;

internal sealed class PlayerJoinedLogic : ComponentLogic<UserComponent>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEntityEngine _entityEngine;
    private readonly ILogger<PlayerJoinedLogic> _logger;
    private readonly INametagsService _nametagsService;
    private readonly ChatBox _chatBox;
    private readonly Text3dService _text3DService;
    private readonly UserManager<UserData> _userManager;
    private readonly IGuiSystemService? _guiSystemService;

    public PlayerJoinedLogic(IServiceProvider serviceProvider, IEntityEngine entityEngine, ILogger<PlayerJoinedLogic> logger, INametagsService nametagsService,ChatBox chatBox, Text3dService text3DService, UserManager<UserData> userManager, IBrowserGuiService browserGuiService, IGuiSystemService? guiSystemService = null) : base(entityEngine)
    {
        _serviceProvider = serviceProvider;
        _entityEngine = entityEngine;
        _logger = logger;
        _nametagsService = nametagsService;
        _chatBox = chatBox;
        _text3DService = text3DService;
        _userManager = userManager;
        _guiSystemService = guiSystemService;
        _entityEngine.EntityCreated += HandleEntityCreated;
        browserGuiService.Ready += HandleReady;
    }

    private void HandleReady(Entity entity)
    {
        _chatBox.OutputTo(entity, "Browser ready");
    }

    private async Task ComponentAddedCore(UserComponent userComponent)
    {
        var entity = userComponent.Entity;
        var playerElementComponent = userComponent.Entity.GetRequiredComponent<PlayerElementComponent>();
        await playerElementComponent.FadeCameraAsync(CameraFade.Out);
        if (_guiSystemService != null)
            _guiSystemService.SetDebugToolsEnabled(RealmInternal.GetPlayer(playerElementComponent), true);
        _chatBox.SetVisibleFor(entity, true);
        _chatBox.ClearFor(entity);
        playerElementComponent.SetCameraTarget(userComponent.Entity);
        if (!playerElementComponent.TrySpawnAtLastPosition())
        {
            playerElementComponent.Spawn(new Vector3(362.58f + (float)Random.Shared.NextDouble() * 3, -91.07f + (float)Random.Shared.NextDouble() * 3, 1.38f),
                new Vector3(0, 0, 90));
        }
        await Task.Delay(300);
        await playerElementComponent.FadeCameraAsync(CameraFade.In);
        _text3DService.SetRenderingEnabled(entity, true);
        entity.AddComponent(new NametagComponent("KoxKociarz"));
        _nametagsService.SetNametagRenderingEnabled(userComponent.Entity, true);
    }

    protected override async void ComponentAdded(UserComponent userComponent)
    {
        try
        {
            await ComponentAddedCore(userComponent);
        }
        catch (Exception ex)
        {
            _logger.LogHandleError(ex);
        }
    }

    protected override void ComponentDetached(UserComponent userComponent)
    {
        //ShowLoginSequence(userComponent.Entity);
    }

    private void ShowLoginSequence(Entity entity)
    {
        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();

        _text3DService.SetRenderingEnabled(entity, false);
        _chatBox.SetVisibleFor(entity, false);
        _chatBox.ClearFor(entity);
        playerElementComponent.FadeCamera(CameraFade.In);
        playerElementComponent.SetCameraMatrix(new Vector3(379.89844f, -92.6416f, 10.950561f), new Vector3(336.75684f, -93.018555f, 1.3956465f));
        if (!entity.HasComponent<AdminComponent>())
        {
            var adminComponent = entity.AddComponent(new AdminComponent(new List<AdminTool> { AdminTool.Entities, AdminTool.Components, AdminTool.ShowSpawnMarkers }));
            adminComponent.DebugView = true;
            adminComponent.DevelopmentMode = true;
        }

        if(!entity.HasComponent<LoginGuiComponent>())
        {
            var scope = _serviceProvider.CreateScope();
            entity.AddComponent<LoginGuiComponent>(scope.ServiceProvider);
        }

        entity.Disposed += Entity_Disposed;
    }

    private void Entity_Disposed(Entity obj)
    {

        obj.Disposed -= Entity_Disposed;
    }

    private void HandleEntityCreated(Entity entity)
    {
        if (!entity.HasComponent<PlayerTagComponent>())
            return;

        ShowLoginSequence(entity);

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
