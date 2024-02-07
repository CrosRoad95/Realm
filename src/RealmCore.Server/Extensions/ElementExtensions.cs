using RealmCore.Persistence.Data.Helpers;

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

    public static float DistanceTo(this Element a, Element b)
    {
        var length = (a.Position - b.Position).Length();
        return length;
    }

    public static TransformAndMotion GetTransformAndMotion(this Element element) => new()
    {
        Position = element.Position,
        Rotation = element.Rotation,
        Interior = element.Interior,
        Dimension = element.Dimension,
    };
}
