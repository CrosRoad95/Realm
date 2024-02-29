using RealmCore.Server.Modules.Pickups;

namespace RealmCore.Server.Modules.Elements;

public interface IElementFactory
{
    event Action<Element>? ElementCreated;

    void AssociateWithServer(Element element);
    RealmBlip CreateBlip(Location location, BlipIcon blipIcon, Action<RealmBlip>? elementBuilder = null);
    RealmCollisionCircle CreateCollisionCircle(Vector2 position, float radius, byte? interior = null, ushort? dimension = null, Action<RealmCollisionCircle>? elementBuilder = null);
    RealmCollisionCuboid CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte? interior = null, ushort? dimension = null, Action<RealmCollisionCuboid>? elementBuilder = null);
    RealmCollisionPolygon CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte? interior = null, ushort? dimension = null, Action<RealmCollisionPolygon>? elementBuilder = null);
    RealmCollisionRectangle CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte? interior = null, ushort? dimension = null, Action<RealmCollisionRectangle>? elementBuilder = null);
    RealmCollisionSphere CreateCollisionSphere(Vector3 position, float radius, byte? interior = null, ushort? dimension = null, Action<RealmCollisionSphere>? elementBuilder = null);
    RealmCollisionTube CreateCollisionTube(Vector3 position, float radius, float height, byte? interior = null, ushort? dimension = null, Action<RealmCollisionTube>? elementBuilder = null);
    FocusableRealmWorldObject CreateFocusableObject(Location location, ObjectModel model, Action<RealmWorldObject>? elementBuilder = null);
    RealmMarker CreateMarker(Location location, MarkerType markerType, float size, Color color, Action<RealmMarker>? elementBuilder = null);
    RealmWorldObject CreateObject(Location location, ObjectModel model, Action<RealmWorldObject>? elementBuilder = null);
    RealmPed CreatePed(Location location, PedModel pedModel, Action<RealmPed>? elementBuilder = null);
    RealmPickup CreatePickup(Location location, ushort model, Action<RealmPickup>? elementBuilder = null);
    RealmRadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color, byte? interior = null, ushort? dimension = null, Action<RealmRadarArea>? elementBuilder = null);
    RealmVehicle CreateVehicle(Location location, VehicleModel model, Action<RealmVehicle>? elementBuilder = null);
    void RelayCreated(Element element);
}
