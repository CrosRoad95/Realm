namespace RealmCore.Console.Logic;

internal class ServerLogic
{
    private readonly IOptions<GameplayOptions> _gameplayOptions;

    public ServerLogic(IOptions<GameplayOptions> gameplayOptions, MtaServer mtaServer)
    {
        _gameplayOptions = gameplayOptions;
        if (!string.IsNullOrWhiteSpace(_gameplayOptions.Value.Password))
            mtaServer.Password = _gameplayOptions.Value.Password;
    }
}
