namespace Realm.Server.Interfaces;

public interface IEntityFactory
{
    Entity CreateCollisionSphere(Vector3 position, float radius, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null);
    Entity CreateMarker(MarkerType markerType, Vector3 position, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null);
    Entity CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null);
}
