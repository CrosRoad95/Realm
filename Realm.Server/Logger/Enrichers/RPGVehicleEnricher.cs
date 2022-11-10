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
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("this", _rpgVehicle));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("padding", ": "));
    }
}
