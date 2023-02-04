namespace Realm.Server.Extensions;

public static class EntityExtensions
{
    public static bool IsLookingAt(this Entity a, Entity b, float tolerance = 25.0f)
    {
        if (a.Tag != Entity.EntityTag.Player)
            throw new InvalidOperationException();

        var t =  (a.Transform.Position.FindRotation(b.Transform.Position) + a.Transform.Rotation.Z) % 360;
        if (t > 180)
            t -= 360;
        return Math.Abs(t) < tolerance;
    }

    public static float DistanceTo(this Entity a, Entity b)
    {
        var length = (a.Transform.Position - b.Transform.Position).Length();
        return length;
    }
}
