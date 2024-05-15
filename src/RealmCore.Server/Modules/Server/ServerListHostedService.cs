namespace RealmCore.Server.Modules.Server;

internal sealed class ServerListHostedService : IHostedService
{
    private readonly MtaServer _server;
    private readonly IOptionsMonitor<GameplayOptions> _gameplayOptions;
    private readonly IOptionsMonitor<ServerListOptions> _serverListOptions;
    private readonly ILogger<ServerListHostedService> _logger;
    private readonly Debounce _debounce = new(500);

    public ServerListHostedService(MtaServer server, IOptionsMonitor<GameplayOptions> gameplayOptions, IOptionsMonitor<ServerListOptions> serverListOptions, ILogger<ServerListHostedService> logger)
    {
        _server = server;
        _gameplayOptions = gameplayOptions;
        _serverListOptions = serverListOptions;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        UpdateGameplayOptions(true);
        UpdateServerListOptions(true);
        _gameplayOptions.OnChange(GameplayOptionsChanged);
        _serverListOptions.OnChange(ServerListOptionsChanged);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void UpdateGameplayOptions(bool firstUpdate = false)
    {
        if (_gameplayOptions.CurrentValue.Password == _server.Password)
            return;

        if (!string.IsNullOrWhiteSpace(_gameplayOptions.CurrentValue.Password))
            _server.Password = _gameplayOptions.CurrentValue.Password;
        else
            _server.Password = null;

        if (!firstUpdate)
            _logger.LogInformation("Gameplay options updated. Password={serverPassword}", _server.Password);
    }

    private void UpdateServerListOptions(bool firstUpdate = false)
    {
        var value = _serverListOptions.CurrentValue;
        if (value.GameType == _server.GameType && value.MapName == _server.MapName)
            return;

        _server.GameType = _serverListOptions.CurrentValue.GameType;
        _server.MapName = _serverListOptions.CurrentValue.MapName;

        if (!firstUpdate)
            _logger.LogInformation("Server list options updated. GameType={gameType}, MapName={mapName}", _server.GameType, _server.MapName);
    }

    private void GameplayOptionsChanged(GameplayOptions gameplayOptions)
    {
        _debounce.Invoke(() =>
        {
            UpdateGameplayOptions();
        });
    }

    private void ServerListOptionsChanged(ServerListOptions serverListOptions)
    {
        _debounce.Invoke(() =>
        {
            UpdateServerListOptions();
        });
    }
}
