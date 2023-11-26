namespace RealmCore.Server.Exceptions;

public class PersistantVehicleAlreadySpawnedException : Exception
{
    public PersistantVehicleAlreadySpawnedException(string message) : base(message) { }
    public PersistantVehicleAlreadySpawnedException() { }
}
