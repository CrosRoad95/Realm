namespace Realm.Server.Logger.Enrichers;

internal class SpawnEnricher : ILogEventEnricher
{
    private readonly Spawn _spawn;

    public SpawnEnricher(Spawn rpgVehicle)
    {
        _spawn = rpgVehicle;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("vehicleId", _spawn.Id));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("userFriendlyName", _spawn.LongUserFriendlyName()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("padding", ": "));
    }
}
