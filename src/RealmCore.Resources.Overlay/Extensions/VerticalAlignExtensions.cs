using RealmCore.Resources.Overlay.Enums;

namespace RealmCore.Resources.Overlay.Extensions;

internal static class VerticalAlignExtensions
{
    public static string AsString(this VerticalAlign verticalAlign) => verticalAlign switch
    {
        VerticalAlign.Top => "top",
        VerticalAlign.Bottom => "bottom",
        VerticalAlign.Center => "center",
        _ => throw new NotImplementedException()
    };
}
