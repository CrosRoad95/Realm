

namespace RealmCore.Server.Interfaces;

public interface IElementFactory
{
    event Action<Element>? ElementCreated;

    RealmBlip CreateBlip(Vector3 position, BlipIcon blipIcon, byte? interior = null, ushort? dimension = null, Func<RealmBlip, IEnumerable<IComponent>>? elementBuilder = null);
    RealmCollisionCircle CreateCollisionCircle(Vector2 position, float radius, byte? interior = null, ushort? dimension = null, Func<RealmCollisionCircle, IEnumerable<IComponent>>? elementBuilder = null);
    RealmCollisionCuboid CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte? interior = null, ushort? dimension = null, Func<RealmCollisionCuboid, IEnumerable<IComponent>>? elementBuilder = null);
    RealmCollisionPolygon CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte? interior = null, ushort? dimension = null, Func<RealmCollisionPolygon, IEnumerable<IComponent>>? elementBuilder = null);
    RealmCollisionRectangle CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte? interior = null, ushort? dimension = null, Func<RealmCollisionRectangle, IEnumerable<IComponent>>? elementBuilder = null);
    RealmCollisionSphere CreateCollisionSphere(Vector3 position, float radius, byte? interior = null, ushort? dimension = null, Func<RealmCollisionSphere, IEnumerable<IComponent>>? elementBuilder = null);
    RealmCollisionTube CreateCollisionTube(Vector3 position, float radius, float height, byte? interior = null, ushort? dimension = null, Func<RealmCollisionTube, IEnumerable<IComponent>>? elementBuilder = null);
    RealmMarker CreateMarker(Vector3 position, MarkerType markerType, Color color, byte? interior = null, ushort? dimension = null, Func<RealmMarker, IEnumerable<IComponent>>? elementBuilder = null);
    RealmObject CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte? interior = null, ushort? dimension = null, Func<RealmObject, IEnumerable<IComponent>>? elementBuilder = null);
    RealmPed CreatePed(PedModel pedModel, Vector3 position, byte? interior = null, ushort? dimension = null, Func<RealmPed, IEnumerable<IComponent>>? elementBuilder = null);
    RealmPickup CreatePickup(Vector3 position, ushort model, byte? interior = null, ushort? dimension = null, Func<RealmPickup, IEnumerable<IComponent>>? elementBuilder = null);
    RealmRadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color, byte? interior = null, ushort? dimension = null, Func<RealmRadarArea, IEnumerable<IComponent>>? elementBuilder = null);
    RealmVehicle CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte? interior = null, ushort? dimension = null, Func<RealmVehicle, IEnumerable<IComponent>>? elementBuilder = null);
    internal void RelayCreated(Element element);
}
