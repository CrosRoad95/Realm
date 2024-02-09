namespace RealmCore.Server.Modules.Persistence;

public class PersistantVehicleAlreadySpawnedException : Exception
{
    public PersistantVehicleAlreadySpawnedException(string message) : base(message) { }
    public PersistantVehicleAlreadySpawnedException() { }
}
