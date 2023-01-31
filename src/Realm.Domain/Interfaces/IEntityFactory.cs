using Realm.Domain.Components.CollisionShapes;
using SlipeServer.Server.Enums;

namespace Realm.Domain.Interfaces;

public interface IEntityFactory
{
    Entity CreateCollisionSphere(Vector3 position, float radius, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null);
    Entity CreateMarker(MarkerType markerType, Vector3 position, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null);
    Task<Entity> CreateNewPrivateVehicle(ushort model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null);
    Entity CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null);
    BlipElementComponent CreateBlipFor(Entity entity, BlipIcon blipIcon, Vector3 position);
    CollisionSphereElementComponent CreateCollisionSphereFor(Entity entity, Vector3 position, float radius);
    MarkerElementComponent CreateMarkerFor(Entity entity, Vector3 position, MarkerType markerType, System.Drawing.Color? color = null);
    Entity CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, string? id = null, Action<Entity>? entityBuilder = null);
    Entity CreateBlip(BlipIcon blipIcon, Vector3 position, string? id = null);
    Entity CreatePickup(ushort model, Vector3 position, string? id = null);
}
