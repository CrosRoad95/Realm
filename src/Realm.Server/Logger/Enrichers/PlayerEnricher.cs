namespace Realm.Server.Logger.Enrichers;

internal class PlayerEnricher : ILogEventEnricher
{
    private readonly Player _player;

    public PlayerEnricher(Player player)
    {
        _player = player;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("serial", _player.Client.Serial));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ip", _player.Client.IPAddress));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("playerId", _player.Id));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("playerName", _player.Name));
    }
}
