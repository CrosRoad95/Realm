namespace RealmCore.Server.Modules.Persistence;

public class PersistantVehicleNotFoundException : Exception
{
    public PersistantVehicleNotFoundException(string message) : base(message) { }
    public PersistantVehicleNotFoundException() { }
}
