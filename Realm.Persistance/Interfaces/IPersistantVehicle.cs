namespace Realm.Persistance.Interfaces;

public interface IPersistantVehicle : ISavable
{
    string VehicleId { get; }

    event Action<IPersistantVehicle>? NotifyNotSavedState;
    event Action<IPersistantVehicle>? Disposed;
}
