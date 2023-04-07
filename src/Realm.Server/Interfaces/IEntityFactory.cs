using SlipeServer.Server.Enums;

namespace Realm.Server.Interfaces;

public interface IEntityFactory
{
    Entity CreateCollisionSphere(Vector3 position, float radius, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreateMarker(MarkerType markerType, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Task<Entity> CreateNewPrivateVehicle(ushort model, Vector3 position, Vector3 rotation, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreateVehicle(ushort model, Vector3 position, Vector3 rotation, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    PlayerPrivateElementComponent<BlipElementComponent> CreateBlipFor(Entity entity, BlipIcon blipIcon, Vector3 position);
    PlayerPrivateElementComponent<CollisionSphereElementComponent> CreateCollisionSphereFor(Entity entity, Vector3 position, float radius);
    PlayerPrivateElementComponent<MarkerElementComponent> CreateMarkerFor(Entity entity, Vector3 position, MarkerType markerType, Color? color = null);
    Entity CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreateBlip(BlipIcon blipIcon, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreatePickup(ushort model, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreatePed(PedModel pedModel, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreateRadarArea(Vector2 position, Vector2 size, Color color, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    PlayerPrivateElementComponent<RadarAreaElementComponent> CreateRadarAreaFor(Entity playerEntity, Vector2 position, Vector2 size, Color color);
    PlayerPrivateElementComponent<WorldObjectComponent> CreateObjectFor(Entity playerEntity, ObjectModel model, Vector3 position, Vector3 rotation);
}
