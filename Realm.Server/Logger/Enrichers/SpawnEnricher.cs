using Realm.Domain.Elements;

namespace Realm.Server.Logger.Enrichers;

internal class SpawnEnricher : ILogEventEnricher
{
    private readonly RPGSpawn _spawn;

    public SpawnEnricher(RPGSpawn rpgVehicle)
    {
        _spawn = rpgVehicle;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("vehicleId", _spawn.Id));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("spawnName", _spawn.Name));
    }
}
