namespace RealmCore.Server.Modules.Players.Gui.Dx;

internal sealed class WatermarkResourceLogic
{
    private readonly WatermarkService _watermarkService;

    public WatermarkResourceLogic(WatermarkService watermarkService, IOptionsMonitor<GameplayOptions> gameplayOptions)
    {
        watermarkService.SetContent(gameplayOptions.CurrentValue.Watermark);
        _watermarkService = watermarkService;
        gameplayOptions.OnChange(HandleGameplayOptionsChanged);
    }

    private void HandleGameplayOptionsChanged(GameplayOptions gameplayOptions)
    {
        _watermarkService.SetContent(gameplayOptions.Watermark);
    }
}
