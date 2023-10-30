using RealmCore.Persistence.Data.Helpers;

namespace RealmCore.Server.Extensions;

public static class ElementExtensions
{
    public static void ThrowIfDestroyed(this Element element)
    {
        if (element.IsDestroyed)
            throw new ElementDestroyedException();
    }

    public static TransformAndMotion GetTransformAndMotion(this Element element) => new()
    {
        Position = element.Position,
        Rotation = element.Rotation,
        Interior = element.Interior,
        Dimension = element.Dimension,
    };

    public static Entity UpCast(this Element element) => ((IElementComponent)element).Entity;
    public static Entity? TryUpCast(this Element element)
    {
        if (element is IElementComponent elementComponent)
            return elementComponent.Entity;
        return null;
    }
}
