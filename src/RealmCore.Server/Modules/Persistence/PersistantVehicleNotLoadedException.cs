namespace RealmCore.Server.Modules.Persistence;

public class PersistantVehicleNotLoadedException : Exception
{
    public PersistantVehicleNotLoadedException(string message) : base(message) { }
    public PersistantVehicleNotLoadedException() { }
}
