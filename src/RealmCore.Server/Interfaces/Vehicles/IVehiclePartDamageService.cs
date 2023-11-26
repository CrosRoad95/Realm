namespace RealmCore.Server.Interfaces.Vehicles;

public interface IVehiclePartDamageService : IVehicleService
{
    IReadOnlyList<short> Parts { get; }

    event Action<IVehiclePartDamageService, short>? PartDestroyed;

    void AddPart(short partId, float state);
    float GetState(short partId);
    bool HasPart(short partId);
    void Modify(short partId, float difference);
    void RemovePart(short partId);
    bool TryGetState(short partId, out float state);
}
