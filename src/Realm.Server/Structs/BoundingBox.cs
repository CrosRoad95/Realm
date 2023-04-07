namespace Realm.Server.Structs;

public struct BoundingBox
{
    public Vector3 center;
    public Vector3 extents;

    public BoundingBox(Vector3 center, Vector3 extents)
    {
        this.center = center;
        this.extents = extents;
    }
}
