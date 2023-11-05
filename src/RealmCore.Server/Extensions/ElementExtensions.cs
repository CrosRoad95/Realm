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

    public static TComponent GetRequiredComponent<TComponent>(this Element element) where TComponent : IComponent
    {
        return ((IComponents)element).GetRequiredComponent<TComponent>();
    }

    public static bool TryDestroyComponent<TComponent>(this Element element) where TComponent : IComponent
    {
        return ((IComponents)element).TryDestroyComponent<TComponent>();
    }

    public static void DestroyComponent<TComponent>(this Element element) where TComponent : IComponent
    {
        ((IComponents)element).DestroyComponent<TComponent>();
    }

    public static void DestroyComponent<TComponent>(this Element element, TComponent component) where TComponent : IComponent
    {
        ((IComponents)element).DestroyComponent(component);
    }

    public static bool TryGetComponent<TComponent>(this Element element, out TComponent component) where TComponent : IComponent
    {
        if (((IComponents)element).TryGetComponent(out TComponent tempComponent))
        {
            component = tempComponent;
            return true;
        }
        component = default!;
        return false;
    }

    public static bool HasComponent<TComponent>(this Element element) where TComponent : IComponent
    {
        return ((IComponents)element).HasComponent<TComponent>();
    }

    public static TComponent AddComponent<TComponent>(this Element element) where TComponent : IComponent, new()
    {
        return ((IComponents)element).AddComponent<TComponent>();
    }

    public static TComponent AddComponent<TComponent>(this Element element, TComponent component) where TComponent : IComponent
    {
        return ((IComponents)element).AddComponent(component);
    }

    public static TComponent AddComponentWithDI<TComponent>(this Element element, params object[] parameters) where TComponent : IComponent
    {
        return ((IComponents)element).AddComponentWithDI<TComponent>(parameters);
    }
}
