namespace Realm.Server.Extensions;

public static class EntityExtensions
{
    public static bool IsLookingAt(this Entity a, Entity b, float tolerance = 15.0f)
    {
        if (a.Tag != Entity.EntityTag.Player)
            throw new InvalidOperationException();

        var t = Math.Abs(a.Transform.Position.FindRotation(b.Transform.Position) + a.Transform.Rotation.Z);
        return t < tolerance;
    }
}
