using RealmCore.Server.Components.Elements.Abstractions;

namespace RealmCore.Server.Extensions;

public static class EntityExtensions
{
    public static bool IsLookingAt(this Entity a, Entity b, float tolerance = 25.0f)
    {
        var tag = a.GetRequiredComponent<TagComponent>();
        if (tag is not PlayerTagComponent)
            throw new InvalidOperationException();

        var t = (a.Transform.Position.FindRotation(b.Transform.Position) + a.Transform.Rotation.Z) % 360;
        if (t > 180)
            t -= 360;
        return Math.Abs(t) < tolerance;
    }

    public static float DistanceTo(this Entity a, Entity b)
    {
        var length = (a.Transform.Position - b.Transform.Position).Length();
        return length;
    }

    internal static bool TryGetElement(this Entity entity, out Element element)
    {
        if (entity.TryGetComponent(out ElementComponent elementComponent))
        {
            element = elementComponent.Element;
            return true;
        }

        element = null!;
        return false;
    }

    internal static Player GetPlayer(this Entity entity) => entity.GetRequiredComponent<PlayerElementComponent>().Player;
    internal static Vehicle GetVehicle(this Entity entity) => entity.GetRequiredComponent<VehicleElementComponent>().Vehicle;
    internal static Element GetElement(this Entity entity) => entity.GetRequiredComponent<ElementComponent>().Element;
}
