using Realm.Domain.Concepts;
using SlipeServer.Server.Enums;

namespace Realm.Domain.Interfaces;

public interface IEntityFactory
{
    Entity CreateCollisionSphere(Vector3 position, float radius, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreateMarker(MarkerType markerType, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Task<Entity> CreateNewPrivateVehicle(ushort model, Vector3 position, Vector3 rotation, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreateVehicle(ushort model, Vector3 position, Vector3 rotation, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    BlipElementComponent CreateBlipFor(Entity entity, BlipIcon blipIcon, Vector3 position);
    CollisionSphereElementComponent CreateCollisionSphereFor(Entity entity, Vector3 position, float radius);
    MarkerElementComponent CreateMarkerFor(Entity entity, Vector3 position, MarkerType markerType, Color? color = null);
    Entity CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreateBlip(BlipIcon blipIcon, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreatePickup(ushort model, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreatePed(PedModel pedModel, Vector3 position, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    Entity CreateRadarArea(Vector2 position, Vector2 size, Color color, ConstructionInfo? constructionInfo = null, Action<Entity>? entityBuilder = null);
    RadarAreaElementComponent CreateRadarAreaFor(Entity playerEntity, Vector2 position, Vector2 size, Color color);
    WorldObjectComponent CreateObjectFor(Entity playerEntity, ObjectModel model, Vector3 position, Vector3 rotation);
}
