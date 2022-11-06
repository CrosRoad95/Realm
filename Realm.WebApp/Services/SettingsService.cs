using Realm.Server.Interfaces;
using SlipeServer.Server.Services;

namespace Realm.WebApp.Services;

public class SettingsService
{
    private readonly IRPGServer _server;
    private readonly GameWorld _gameWorld;
    private readonly SlipeServer.Server.Configuration _serverConfiguration;

    public string ServerName
    {
        get => _serverConfiguration.ServerName;
        set => throw new NotSupportedException();
    }

    public int FpsLimit
    {
        get => _gameWorld.FpsLimit;
        set => _gameWorld.FpsLimit = (byte)value;
    }

    public int Weather
    {
        get => _gameWorld.Weather;
        set => _gameWorld.SetWeather((byte)value);
    }
    
    public string GameType
    {
        get => _server.GameType;
        set => _server.GameType = value;
    }
    
    
    public string MapName
    {
        get => _server.MapName;
        set => _server.MapName = value;
    }

    public SettingsService(IRPGServer server)
    {
        _server = server;
        _gameWorld = _server.GetRequiredService<GameWorld>();
        var configurationProvider = _server.GetRequiredService<Realm.Configuration.ConfigurationProvider>();
        _serverConfiguration = configurationProvider.Get<SlipeServer.Server.Configuration>("Server");
    }
}
