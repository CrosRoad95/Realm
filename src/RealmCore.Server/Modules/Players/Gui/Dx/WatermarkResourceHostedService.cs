namespace RealmCore.Server.Modules.Players.Gui.Dx;

internal sealed class WatermarkResourceHostedService : IHostedService
{
    private readonly WatermarkService _watermarkService;
    private readonly IOptionsMonitor<GameplayOptions> _gameplayOptions;

    public WatermarkResourceHostedService(WatermarkService watermarkService, IOptionsMonitor<GameplayOptions> gameplayOptions)
    {
        _watermarkService = watermarkService;
        _gameplayOptions = gameplayOptions;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _watermarkService.SetContent(_gameplayOptions.CurrentValue.Watermark);
        _gameplayOptions.OnChange(HandleGameplayOptionsChanged);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void HandleGameplayOptionsChanged(GameplayOptions gameplayOptions)
    {
        _watermarkService.SetContent(gameplayOptions.Watermark);
    }
}
