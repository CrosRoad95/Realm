namespace Realm.Server.Interfaces;

public interface IEntityFactory
{
    Entity CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null);
}
