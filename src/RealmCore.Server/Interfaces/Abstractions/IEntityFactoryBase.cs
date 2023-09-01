namespace RealmCore.Server.Interfaces.Abstractions;

public interface IEntityFactoryBase
{
    Entity CreateCollisionSphere(Vector3 position, float radius, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null);
    Entity CreateMarker(MarkerType markerType, Vector3 position, Color color, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null);
    Entity CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null);
    Entity CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null);
    Entity CreateBlip(BlipIcon blipIcon, Vector3 position, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null);
    Entity CreatePickup(ushort model, Vector3 position, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null);
    Entity CreatePed(PedModel pedModel, Vector3 position, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null);
    Entity CreateRadarArea(Vector2 position, Vector2 size, Color color, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null);
    Entity CreateCollisionCircle(Vector2 position, float radius, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null);
    Entity CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null);
    Entity CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null);
    Entity CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null);
    Entity CreateCollisionTube(Vector3 position, float radius, float height, byte interior = 0, ushort dimension = 0, Action<Entity>? entityBuilder = null);
}
