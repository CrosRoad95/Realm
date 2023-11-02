

namespace RealmCore.Server.Interfaces;

public interface IElementFactory
{
    event Action<Element>? ElementCreated;

    RealmBlip CreateBlip(Vector3 position, BlipIcon blipIcon, byte? interior = null, ushort? dimension = null, Func<RealmBlip, IEnumerable<IComponent>>? elementBuilder = null);
    CollisionCircle CreateCollisionCircle(Vector2 position, float radius, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null);
    CollisionCuboid CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null);
    CollisionPolygon CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null);
    CollisionRectangle CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null);
    RealmCollisionSphere CreateCollisionSphere(Vector3 position, float radius, byte? interior = null, ushort? dimension = null, Func<RealmCollisionSphere, IEnumerable<IComponent>>? elementBuilder = null);
    CollisionTube CreateCollisionTube(Vector3 position, float radius, float height, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null);
    RealmMarker CreateMarker(Vector3 position, MarkerType markerType, Color color, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null);
    RealmObject CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte? interior = null, ushort? dimension = null, Func<RealmObject, IEnumerable<IComponent>>? elementBuilder = null);
    Ped CreatePed(PedModel pedModel, Vector3 position, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null);
    RealmPickup CreatePickup(Vector3 position, ushort model, byte? interior = null, ushort? dimension = null, Func<RealmPickup, IEnumerable<IComponent>>? elementBuilder = null);
    RadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color, byte? interior = null, ushort? dimension = null, Action<Element>? elementBuilder = null);
    RealmVehicle CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte? interior = null, ushort? dimension = null, Func<RealmVehicle, IEnumerable<IComponent>>? elementBuilder = null);
    internal void RelayCreated(Element element);
}
