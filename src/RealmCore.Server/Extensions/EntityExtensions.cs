namespace RealmCore.Server.Extensions;

public static class EntityExtensions
{
    public static bool IsLookingAt(this Entity a, Entity b, float tolerance = 25.0f)
    {
        var tag = a.GetRequiredComponent<TagComponent>();
        if (tag is not PlayerTagComponent)
            throw new InvalidOperationException();

        var elementA = (Element)a.GetRequiredComponent<IElementComponent>();
        var elementB = (Element)b.GetRequiredComponent<IElementComponent>();
        var t = (elementA.Position.FindRotation(elementB.Position) + elementA.Rotation.Z) % 360;
        if (t > 180)
            t -= 360;
        return Math.Abs(t) < tolerance;
    }

    public static TComponent AddComponentWithDI<TComponent>(this Entity entity, params object[] parameters) where TComponent : Component
    {
        var realmPlayer = (RealmPlayer)entity.GetRequiredComponent<PlayerElementComponent>();
        return entity.AddComponent(ActivatorUtilities.CreateInstance<TComponent>(realmPlayer.ServiceProvider, parameters));
    }

    public static bool TryGetElement(this Entity entity, out Element element)
    {
        if (entity.TryGetComponent(out IElementComponent elementComponent))
        {
            element = (Element)elementComponent;
            return true;
        }

        element = null!;
        return false;
    }

    public static Element GetElement(this Entity entity) => (Element)entity.GetRequiredComponent<IElementComponent>();
}
