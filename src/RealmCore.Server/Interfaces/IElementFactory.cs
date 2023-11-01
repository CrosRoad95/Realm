

namespace RealmCore.Server.Interfaces;

public interface IElementFactory
{
    event Action<Element>? ElementCreated;

    Blip CreateBlip(Vector3 position, BlipIcon blipIcon, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null);
    CollisionCircle CreateCollisionCircle(Vector2 position, float radius, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null);
    CollisionCuboid CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null);
    CollisionPolygon CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null);
    CollisionRectangle CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null);
    RealmCollisionSphere CreateCollisionSphere(Vector3 position, float radius, byte interior = 0, ushort dimension = 0, Action<RealmCollisionSphere>? elementBuilder = null);
    CollisionTube CreateCollisionTube(Vector3 position, float radius, float height, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null);
    RealmMarker CreateMarker(Vector3 position, MarkerType markerType, Color color, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null);
    RealmObject CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, Action<RealmObject>? elementBuilder = null);
    Ped CreatePed(PedModel pedModel, Vector3 position, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null);
    RealmPickup CreatePickup(Vector3 position, ushort model, byte interior = 0, ushort dimension = 0, Action<RealmPickup>? elementBuilder = null);
    RadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color, byte interior = 0, ushort dimension = 0, Action<Element>? elementBuilder = null);
    RealmVehicle CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte interior = 0, ushort dimension = 0, Action<RealmVehicle>? elementBuilder = null);
}
