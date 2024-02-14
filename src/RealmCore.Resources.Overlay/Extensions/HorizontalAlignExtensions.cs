namespace RealmCore.Resources.Overlay.Extensions;

internal static class HorizontalAlignExtensions
{
    public static string AsString(this HorizontalAlign horizontalAlign) => horizontalAlign switch
    {
        HorizontalAlign.Left => "left",
        HorizontalAlign.Right => "right",
        HorizontalAlign.Center => "center",
        _ => throw new NotImplementedException()
    };
}
