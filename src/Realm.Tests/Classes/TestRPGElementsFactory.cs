using Realm.Domain.Elements;
using Realm.Domain.Elements.CollisionShapes;
using Realm.Domain.Elements.Variants;
using Realm.Server.Interfaces;
using System.Drawing;
using System.Numerics;

namespace Realm.Tests.Classes;

internal class TestRPGElementsFactory : IRPGElementsFactory
{
    public event Action<RPGSpawn>? SpawnCreated;
    public event Action<RPGVehicle>? VehicleCreated;
    public event Action<RPGBlip>? BlipCreated;
    public event Action<RPGFraction>? FractionCreated;

    public RPGBlip CreateBlip(Vector3 position, int icon)
    {
        throw new NotImplementedException();
    }

    public RPGCollisionSphere CreateColSphere(Vector3 position, float radius)
    {
        throw new NotImplementedException();
    }

    public RPGFraction CreateFraction(string code, string name, Vector3 position)
    {
        throw new NotImplementedException();
    }

    public Task<RPGVehicle> CreateNewPersistantVehicle(string id, ushort model, RPGSpawn spawn)
    {
        throw new NotImplementedException();
    }

    public RPGPickup CreatePickup(Vector3 position, ushort model)
    {
        throw new NotImplementedException();
    }

    public RPGRadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color)
    {
        throw new NotImplementedException();
    }

    public RPGSpawn CreateSpawn(Vector3 position, Vector3? rotation = null, string? name = null)
    {
        throw new NotImplementedException();
    }

    public RPGVariantBlip CreateVariantBlip(Vector3 position)
    {
        throw new NotImplementedException();
    }

    public RPGVariantRadarArea CreateVariantRadarArea(Vector2 position, Vector2 size)
    {
        throw new NotImplementedException();
    }

    public RPGVariantPickup CreateVariantRadarArea(Vector3 position, ushort model)
    {
        throw new NotImplementedException();
    }

    public RPGVehicle CreateVehicle(ushort model, RPGSpawn rpgSpawn)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsVehicleIdAvailiable(string id)
    {
        throw new NotImplementedException();
    }

    public Task<RPGVehicle> SpawnPersistantVehicle(string id, RPGSpawn spawn)
    {
        throw new NotImplementedException();
    }
}
