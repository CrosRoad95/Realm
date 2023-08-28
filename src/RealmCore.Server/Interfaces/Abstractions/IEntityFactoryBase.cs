namespace RealmCore.Server.Interfaces.Abstractions;

public interface IEntityFactoryBase
{
    Entity CreateCollisionSphere(Vector3 position, float radius, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreateMarker(MarkerType markerType, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreateVehicle(ushort model, Vector3 position, Vector3 rotation, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreateBlip(BlipIcon blipIcon, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreatePickup(ushort model, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreatePed(PedModel pedModel, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreateRadarArea(Vector2 position, Vector2 size, Color color, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreateCollisionCircle(Vector2 position, float radius, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreateCollisionCuboid(Vector3 position, Vector3 dimensions, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreateCollisionRectangle(Vector2 position, Vector2 dimensions, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreateCollisionTube(Vector3 position, float radius, float height, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
}
