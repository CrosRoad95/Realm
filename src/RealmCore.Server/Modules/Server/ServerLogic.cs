namespace RealmCore.Server.Modules.Server;

internal sealed class ServerLogic
{
    private readonly MtaServer _mtaServer;
    private readonly IOptionsMonitor<GameplayOptions> _gameplayOptions;
    private readonly IOptionsMonitor<ServerListOptions> _serverListOptions;
    private readonly ILogger<ServerLogic> _logger;

    public ServerLogic(MtaServer mtaServer, IOptionsMonitor<GameplayOptions> gameplayOptions, IOptionsMonitor<ServerListOptions> serverListOptions, ILogger<ServerLogic> logger)
    {
        _mtaServer = mtaServer;
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
            _mtaServer.Password = _gameplayOptions.CurrentValue.Password;
        else
            _mtaServer.Password = null;

        if (!firstUpdate)
            _logger.LogInformation("Gameplay options updated. Password={serverPassword}", _mtaServer.Password);
    }

    private void UpdateServerListOptions(bool firstUpdate = false)
    {
        _mtaServer.GameType = _serverListOptions.CurrentValue.GameType;
        _mtaServer.MapName = _serverListOptions.CurrentValue.MapName;

        if (!firstUpdate)
            _logger.LogInformation("Server list options updated. GameType={gameType}, MapName={mapName}", _mtaServer.GameType, _mtaServer.MapName);
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
