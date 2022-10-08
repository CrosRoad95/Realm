namespace Realm.Scripting.Extensions;

internal static class StringExtensions
{
    public static string ToTypescriptName(this string str)
    {
        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}
