using System.Drawing;

namespace RealmCore.Resources.Base.Extensions;

public static class ColorExtensions
{
    public static double ToLuaColor(this Color color) => color.B + color.G * 256 + color.R * 256 * 256 + color.A * 256 * 256 * 256;
}
