
namespace RealmCore.Server.Extensions;

public static class IComponentExtensions
{
    public static TService GetRequiredService<TService>(this IComponent thisComponent) where TService : notnull
    {
        return ((RealmPlayer)thisComponent.Element).GetRequiredService<TService>();
    }

    public static TComponent GetRequiredComponent<TComponent>(this IComponent thisComponent) where TComponent : IComponent
    {
        return ((IComponents)thisComponent.Element).GetRequiredComponent<TComponent>();
    }

    public static bool TryDestroyComponent<TComponent>(this IComponent thisComponent) where TComponent : IComponent
    {
        return ((IComponents)thisComponent.Element).TryDestroyComponent<TComponent>();
    }

    public static void DestroyComponent<TComponent>(this IComponent thisComponent) where TComponent : IComponent
    {
        ((IComponents)thisComponent.Element).DestroyComponent<TComponent>();
    }

    public static void DestroyComponent<TComponent>(this IComponent thisComponent, TComponent component) where TComponent : IComponent
    {
        ((IComponents)thisComponent.Element).DestroyComponent(component);
    }

    public static bool TryGetComponent<TComponent>(this IComponent thisComponent, out TComponent component) where TComponent : IComponent
    {
        if (((IComponents)thisComponent.Element).TryGetComponent(out TComponent tempComponent))
        {
            component = tempComponent;
            return true;
        }
        component = default!;
        return false;
    }

    public static bool HasComponent<TComponent>(this IComponent thisComponent) where TComponent : IComponent
    {
        return ((IComponents)thisComponent.Element).HasComponent<TComponent>();
    }

    public static TComponent AddComponent<TComponent>(this IComponent thisComponent) where TComponent : IComponent, new()
    {
        return ((IComponents)thisComponent.Element).AddComponent<TComponent>();
    }

    public static TComponent AddComponent<TComponent>(this IComponent thisComponent, TComponent component) where TComponent : IComponent
    {
        return ((IComponents)thisComponent.Element).AddComponent(component);
    }

    public static TComponent AddComponentWithDI<TComponent>(this IComponent thisComponent, params object[] parameters) where TComponent : IComponent
    {
        return ((IComponents)thisComponent.Element).AddComponentWithDI<TComponent>(parameters);
    }
}
