using static RealmCore.Server.Extensions.Vector3Extensions;

namespace RealmCore.Server.Extensions;

public static class ElementExtensions
{
    public static bool IsLookingAt(this Element a, Element b, float tolerance = 25.0f)
    {
        var t = (a.Position.FindRotation(b.Position) + a.Rotation.Z) % 360;
        if (t > 180)
            t -= 360;
        return Math.Abs(t) < tolerance;
    }

    public static void LookAt(this Element element, Vector3 position)
    {
        element.Rotation = element.Rotation with { Z = -element.Position.FindRotation(position) };
    }
    
    public static void LookAt(this Element element1, Element element2) => element1.LookAt(element2.Position);

    public static float DistanceTo(this Element a, Element b)
    {
        var length = (a.Position - b.Position).Length();
        return length;
    }
    
    public static float DistanceToSquared(this Element a, Element b)
    {
        var length = (a.Position - b.Position).LengthSquared();
        return length;
    }

    public static TransformAndMotion GetTransformAndMotion(this Element element) => new()
    {
        Position = element.Position,
        Rotation = element.Rotation,
        Interior = element.Interior,
        Dimension = element.Dimension,
    };

    public static Location GetLocation(this Element element, Vector3 offsetPosition = default) => new(element.Position + offsetPosition, element.Rotation, element.Interior, element.Dimension);

    public static void SetLocation(this Element element, Location location, Vector3 offsetPosition = default)
    {
        element.Position = location.Position + offsetPosition;
        element.Rotation = location.Rotation;
        if(location.Dimension != null)
            element.Dimension = location.Dimension.Value;
        if(location.Interior != null)
            element.Interior = location.Interior.Value;
    }

    public static Vector3 GetPointFromDistanceRotation(this Element element, double distance, float? angle = null)
    {
        return element.Position.GetPointFromDistanceRotation(distance, angle ?? -element.Rotation.Z);
    }
    
    public static Vector3 GetPointFromDistanceRotationOffset(this Element element, double distance, float? angle = null)
    {
        double a = Math.PI / 2 - MathUtils.ToRadians(angle ?? -element.Rotation.Z);
        double dx = Math.Cos(a) * distance;
        double dy = Math.Sin(a) * distance;
        return new Vector3((float)dx, (float)dy, 0);
    }

    public static CancellationToken CreateCancellationToken(this Element? element)
    {
        if (element == null)
            throw new NullReferenceException(nameof(element));

        if (element.IsDestroyed)
            return new CancellationToken(true);

        var cancellationTokenSource = new CancellationTokenSource();

        void handleDestroyed(Element element)
        {
            cancellationTokenSource.Cancel();
            element.Destroyed -= handleDestroyed;
        }
        element.Destroyed += handleDestroyed;

        if(element.IsDestroyed)
            cancellationTokenSource.Cancel();

        return cancellationTokenSource.Token;
    }
}
