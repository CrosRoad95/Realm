namespace RealmCore.Server.Extensions;

public static class Vector3Extensions
{
    public static float FindRotation(this Vector3 a, Vector3 b)
    {
        var t = (float)(180 / Math.PI * Math.Atan2(b.X - a.X, b.Y - a.Y));
        return t < 0 ? t + 360 : t;
    }

    public static Vector2 GetPointFromDistanceRotation(this Vector2 vector2, double distance, double angle)
    {
        double a = Math.PI / 2 - MathUtils.ToRadians(angle);
        double dx = Math.Cos(a) * distance;
        double dy = Math.Sin(a) * distance;
        return new Vector2(vector2.X + (float)dx, vector2.Y + (float)dy);
    }

    public static Vector3 GetPointFromDistanceRotation(this Vector3 vector3, double distance, double angle)
    {
        double a = Math.PI / 2 - MathUtils.ToRadians(angle);
        double dx = Math.Cos(a) * distance;
        double dy = Math.Sin(a) * distance;
        return new Vector3(vector3.X + (float)dx, vector3.Y + (float)dy, vector3.Z);
    }

    public static class MathUtils
    {
        public static double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180.0f);
        }

        public static double ToDegrees(double radians)
        {
            return radians * (180.0f / Math.PI);
        }
    }
}
