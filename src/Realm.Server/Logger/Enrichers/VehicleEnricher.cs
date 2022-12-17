namespace Realm.Server.Logger.Enrichers;

internal class VehicleEnricher : ILogEventEnricher
{
    private readonly Vehicle _vehicle;

    public VehicleEnricher(Vehicle vehicle)
    {
        _vehicle = vehicle;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("vehicleId", _vehicle.Id));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("vehicleName", _vehicle.Name));
    }
}
