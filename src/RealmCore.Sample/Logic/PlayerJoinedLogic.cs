using RealmCore.Sample.Components.Gui.Blazor;
using SlipeServer.Server;

namespace RealmCore.Sample.Logic;

internal sealed class PlayerJoinedLogic
{
    private readonly ILogger<PlayerJoinedLogic> _logger;
    private readonly INametagsService _nametagsService;
    private readonly ChatBox _chatBox;
    private readonly Text3dService _text3DService;
    private readonly IGuiSystemService? _guiSystemService;

    public PlayerJoinedLogic(ILogger<PlayerJoinedLogic> logger, INametagsService nametagsService,ChatBox chatBox, Text3dService text3DService, IBrowserGuiService browserGuiService, MtaServer mtaServer, IElementFactory elementFactory, IUsersService usersService, IGuiSystemService? guiSystemService = null)
    {
        _logger = logger;
        _nametagsService = nametagsService;
        _chatBox = chatBox;
        _text3DService = text3DService;
        _guiSystemService = guiSystemService;
        browserGuiService.Ready += HandleReady;
        mtaServer.PlayerJoined += HandlePlayerJoined;
        usersService.SignedIn += HandleSignedIn;
        usersService.SignedOut += HandleSignedOut;
    }

    private void HandleReady(RealmPlayer player)
    {
        _chatBox.OutputTo(player, "Browser ready");
        //player.AddComponent<Counter2GuiComponent>();
    }

    private async Task HandleSignedInCore(RealmPlayer player)
    {
        await player.FadeCameraAsync(CameraFade.Out);
        if (_guiSystemService != null)
            _guiSystemService.SetDebugToolsEnabled(player, true);
        _chatBox.SetVisibleFor(player, true);
        _chatBox.ClearFor(player);
        player.Camera.Target = player;
        if (!player.TrySpawnAtLastPosition())
        {
            player.Spawn(new Vector3(362.58f + (float)Random.Shared.NextDouble() * 3, -91.07f + (float)Random.Shared.NextDouble() * 3, 1.38f),
                new Vector3(0, 0, 90));
        }
        await Task.Delay(300);
        await player.FadeCameraAsync(CameraFade.In);
        _text3DService.SetRenderingEnabled(player, true);
        player.AddComponent(new NametagComponent("KoxKociarz"));
        _nametagsService.SetNametagRenderingEnabled(player, true);
    }

    private async void HandleSignedIn(RealmPlayer player)
    {
        try
        {
            await HandleSignedInCore(player);
        }
        catch (Exception ex)
        {
            _logger.LogHandleError(ex);
        }
    }

    private void HandleSignedOut(RealmPlayer player)
    {
        ShowLoginSequence(player);
    }

    private void ShowLoginSequence(RealmPlayer player)
    {
        _text3DService.SetRenderingEnabled(player, false);
        _chatBox.SetVisibleFor(player, false);
        _chatBox.ClearFor(player);
        player.Camera.Fade(CameraFade.In);
        player.Camera.SetMatrix(new Vector3(379.89844f, -92.6416f, 10.950561f), new Vector3(336.75684f, -93.018555f, 1.3956465f));
        var component = player;
        if (!component.HasComponent<AdminComponent>())
        {
            var adminComponent = component.AddComponent(new AdminComponent(new List<AdminTool> { AdminTool.Elements, AdminTool.Components, AdminTool.ShowSpawnMarkers }));
            adminComponent.DebugView = true;
            adminComponent.DevelopmentMode = true;
        }

        if(!component.HasComponent<LoginGuiComponent>())
        {
            component.Components.AddComponentWithDI<LoginGuiComponent>();
        }
    }

    private void HandlePlayerJoined(Player player)
    {
        ShowLoginSequence((RealmPlayer)player);
    }

    // TODO:
    //private async void HandleComponentAdded(IComponent component)
    //{
    //    try
    //    {
    //        if (component is LevelComponent levelComponent)
    //        {
    //            levelComponent.LevelChanged += (self, level, up) =>
    //            {
    //                _logger.LogInformation("Player leveled up: {level}", level);
    //            };
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Failed to add component.");
    //    }
    //}
}
