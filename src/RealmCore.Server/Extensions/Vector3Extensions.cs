namespace RealmCore.Server.Extensions;

public static class Vector3Extensions
{
    public static float FindRotation(this Vector3 a, Vector3 b)
    {
        var t = (float)(180 / Math.PI * Math.Atan2(b.X - a.X, b.Y - a.Y));
        return t < 0 ? t + 360 : t;
    }
}
