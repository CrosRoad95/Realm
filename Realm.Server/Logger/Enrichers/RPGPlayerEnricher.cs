namespace Realm.Server.Logger.Enrichers;

internal class RPGPlayerEnricher : ILogEventEnricher
{
    private readonly RPGPlayer _rpgPlayer;

    public RPGPlayerEnricher(RPGPlayer rpgPlayer)
    {
        _rpgPlayer = rpgPlayer;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if(_rpgPlayer.Account != null)
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("accountId", _rpgPlayer.Account.Id));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("serial", _rpgPlayer.Client.Serial));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ip", _rpgPlayer.Client.IPAddress));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("playerId", _rpgPlayer.Id));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("playerName", _rpgPlayer.Name));
    }
}
