

namespace RealmCore.Server.Interfaces.Vehicles;

public interface IVehicleEnginesService : IVehicleService
{
    short ActiveEngineId { get; set; }
    List<short> EnginesIds { get; }

    event Action<IVehicleEnginesService, short>? ActiveEngineChanged;
    event Action<IVehicleEnginesService, short>? EngineAdded;
    event Action<IVehicleEnginesService, short>? EngineRemoved;

    void Add(short engineId);
    bool Has(short engineId);
    void Remove(short engineId);
}
