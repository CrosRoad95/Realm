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
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("this", _rpgPlayer));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("padding", ": "));
    }
}
