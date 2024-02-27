namespace RealmCore.Server.Modules.Server;

internal sealed class ServerLogic
{
    private readonly MtaServer _server;
    private readonly IOptionsMonitor<GameplayOptions> _gameplayOptions;
    private readonly IOptionsMonitor<ServerListOptions> _serverListOptions;
    private readonly ILogger<ServerLogic> _logger;

    public ServerLogic(MtaServer server, IOptionsMonitor<GameplayOptions> gameplayOptions, IOptionsMonitor<ServerListOptions> serverListOptions, ILogger<ServerLogic> logger)
    {
        _server = server;
        _gameplayOptions = gameplayOptions;
        _serverListOptions = serverListOptions;
        _logger = logger;

        UpdateGameplayOptions(true);
        UpdateServerListOptions(true);
        _gameplayOptions.OnChange(GameplayOptionsChanged);
        _serverListOptions.OnChange(ServerListOptionsChanged);
    }

    private void UpdateGameplayOptions(bool firstUpdate = false)
    {
        if (!string.IsNullOrWhiteSpace(_gameplayOptions.CurrentValue.Password))
            _server.Password = _gameplayOptions.CurrentValue.Password;
        else
            _server.Password = null;

        if (!firstUpdate)
            _logger.LogInformation("Gameplay options updated. Password={serverPassword}", _server.Password);
    }

    private void UpdateServerListOptions(bool firstUpdate = false)
    {
        _server.GameType = _serverListOptions.CurrentValue.GameType;
        _server.MapName = _serverListOptions.CurrentValue.MapName;

        if (!firstUpdate)
            _logger.LogInformation("Server list options updated. GameType={gameType}, MapName={mapName}", _server.GameType, _server.MapName);
    }

    private void GameplayOptionsChanged(GameplayOptions gameplayOptions)
    {
        UpdateGameplayOptions();
    }

    private void ServerListOptionsChanged(ServerListOptions serverListOptions)
    {
        UpdateServerListOptions();
    }
}
