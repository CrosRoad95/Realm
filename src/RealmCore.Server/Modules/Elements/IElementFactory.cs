namespace RealmCore.Server.Modules.Elements;

public interface IElementFactory
{
    event Action<Element>? ElementCreated;

    void AssociateWithServer(Element element);
    RealmBlip CreateBlip(Vector3 position, BlipIcon blipIcon, byte? interior = null, ushort? dimension = null, Action<RealmBlip>? elementBuilder = null);
    RealmCollisionCircle CreateCollisionCircle(Vector2 position, float radius, byte? interior = null, ushort? dimension = null, Action<RealmCollisionCircle>? elementBuilder = null);
    RealmCollisionCuboid CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte? interior = null, ushort? dimension = null, Action<RealmCollisionCuboid>? elementBuilder = null);
    RealmCollisionPolygon CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte? interior = null, ushort? dimension = null, Action<RealmCollisionPolygon>? elementBuilder = null);
    RealmCollisionRectangle CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte? interior = null, ushort? dimension = null, Action<RealmCollisionRectangle>? elementBuilder = null);
    RealmCollisionSphere CreateCollisionSphere(Vector3 position, float radius, byte? interior = null, ushort? dimension = null, Action<RealmCollisionSphere>? elementBuilder = null);
    RealmCollisionTube CreateCollisionTube(Vector3 position, float radius, float height, byte? interior = null, ushort? dimension = null, Action<RealmCollisionTube>? elementBuilder = null);
    FocusableRealmWorldObject CreateFocusableObject(ObjectModel model, Vector3 position, Vector3 rotation, byte? interior = null, ushort? dimension = null, Action<RealmWorldObject>? elementBuilder = null);
    RealmMarker CreateMarker(Vector3 position, MarkerType markerType, float size, Color color, byte? interior = null, ushort? dimension = null, Action<RealmMarker>? elementBuilder = null);
    RealmWorldObject CreateObject(ObjectModel model, Vector3 position, Vector3 rotation, byte? interior = null, ushort? dimension = null, Action<RealmWorldObject>? elementBuilder = null);
    RealmPed CreatePed(PedModel pedModel, Vector3 position, byte? interior = null, ushort? dimension = null, Action<RealmPed>? elementBuilder = null);
    RealmPickup CreatePickup(Vector3 position, ushort model, byte? interior = null, ushort? dimension = null, Action<RealmPickup>? elementBuilder = null);
    RealmRadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color, byte? interior = null, ushort? dimension = null, Action<RealmRadarArea>? elementBuilder = null);
    RealmVehicle CreateVehicle(ushort model, Vector3 position, Vector3 rotation, byte? interior = null, ushort? dimension = null, Action<RealmVehicle>? elementBuilder = null);
    void RelayCreated(Element element);
}
