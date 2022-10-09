namespace Realm.Common.Extensions;

public static class StringExtensions
{
    public static string ToTypescriptName(this string str)
    {
        return str;
        //return char.ToLowerInvariant(str[0]) + str[1..];
    }
}
