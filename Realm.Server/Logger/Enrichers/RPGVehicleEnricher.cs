using Realm.Server.Elements;

namespace Realm.Server.Logger.Enrichers;

internal class RPGVehicleEnricher : ILogEventEnricher
{
    private readonly RPGVehicle _rpgVehicle;

    public RPGVehicleEnricher(RPGVehicle rpgVehicle)
    {
        _rpgVehicle = rpgVehicle;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("vehicleId", _rpgVehicle.Id));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("userFriendlyName", _rpgVehicle.LongUserFriendlyName()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("padding", ": "));
    }
}
