using Realm.Domain.Elements.CollisionShapes;
using Realm.Domain.Elements.Variants;

namespace Realm.Server.Interfaces;

public interface IRPGElementsFactory
{
    event Action<RPGSpawn>? SpawnCreated;
    event Action<RPGVehicle>? VehicleCreated;
    event Action<RPGBlip>? BlipCreated;
    event Action<RPGFraction>? FractionCreated;

    RPGBlip CreateBlip(Vector3 position, int icon);
    RPGCollisionSphere CreateColSphere(Vector3 position, float radius);
    RPGFraction CreateFraction(string code, string name, Vector3 position);
    Task<RPGVehicle> CreateNewPersistantVehicle(string id, ushort model, RPGSpawn spawn);
    RPGPickup CreatePickup(Vector3 position, ushort model);
    RPGRadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color);
    RPGSpawn CreateSpawn(Vector3 position, Vector3? rotation = null, string? name = null);
    RPGVariantBlip CreateVariantBlip(Vector3 position);
    RPGVariantRadarArea CreateVariantRadarArea(Vector2 position, Vector2 size);
    RPGVariantPickup CreateVariantRadarArea(Vector3 position, ushort model);
    RPGVehicle CreateVehicle(ushort model, RPGSpawn rpgSpawn);
    Task<bool> IsVehicleIdAvailiable(string id);
    Task<RPGVehicle> SpawnPersistantVehicle(string id, RPGSpawn spawn);
}
