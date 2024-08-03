namespace RealmCore.Server.Modules.Server;

public sealed class ServerListOptions
{
    public string GameType { get; set; } = "New-Realm";
    public string MapName { get; set; } = "N/A";
}

internal sealed class ServerListHostedService : IHostedLifecycleService
{
    private readonly MtaServer _server;
    private readonly IOptionsMonitor<GameplayOptions> _gameplayOptions;
    private readonly IOptionsMonitor<ServerListOptions> _serverListOptions;
    private readonly ILogger<ServerListHostedService> _logger;
    private readonly IDebounce _debounce;

    public ServerListHostedService(MtaServer server, IOptionsMonitor<GameplayOptions> gameplayOptions, IOptionsMonitor<ServerListOptions> serverListOptions, ILogger<ServerListHostedService> logger, IDebounceFactory debounceFactory)
    {
        _server = server;
        _gameplayOptions = gameplayOptions;
        _serverListOptions = serverListOptions;
        _logger = logger;
        _debounce = debounceFactory.Create(500);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        GameplayOptionsChanged(_gameplayOptions.CurrentValue);
        ServerListOptionsChanged(_serverListOptions.CurrentValue);
        _gameplayOptions.OnChange(GameplayOptionsChanged);
        _serverListOptions.OnChange(ServerListOptionsChanged);
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void UpdateGameplayOptions(GameplayOptions gameplayOptions)
    {
        if (_gameplayOptions.CurrentValue.Password == _server.Password)
            return;

        if (!string.IsNullOrWhiteSpace(_gameplayOptions.CurrentValue.Password))
            _server.Password = _gameplayOptions.CurrentValue.Password;
        else
            _server.Password = null;

        _logger.LogInformation("Gameplay options updated. Password={serverPassword}", _server.Password);
    }

    private void UpdateServerListOptions(ServerListOptions serverListOptions)
    {
        if (serverListOptions.GameType == _server.GameType && serverListOptions.MapName == _server.MapName)
            return;

        _server.GameType = _serverListOptions.CurrentValue.GameType;
        _server.MapName = _serverListOptions.CurrentValue.MapName;

        _logger.LogInformation("Server list options updated. GameType={gameType}, MapName={mapName}", _server.GameType, _server.MapName);
    }

    private void GameplayOptionsChanged(GameplayOptions gameplayOptions)
    {
        _debounce.Invoke(() =>
        {
            UpdateGameplayOptions(gameplayOptions);
        });
    }

    private void ServerListOptionsChanged(ServerListOptions serverListOptions)
    {
        _debounce.Invoke(() =>
        {
            UpdateServerListOptions(serverListOptions);
        });
    }
}
