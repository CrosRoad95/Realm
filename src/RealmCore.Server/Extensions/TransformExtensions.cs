using RealmCore.Persistence.Data.Helpers;

namespace RealmCore.Server.Extensions;

internal static class TransformExtensions
{
    internal static TransformAndMotion GetTransformAndMotion(this Transform transform) => new()
    {
        Position = transform.Position,
        Rotation = transform.Rotation,
        Interior = transform.Interior,
        Dimension = transform.Dimension,
    };
}
