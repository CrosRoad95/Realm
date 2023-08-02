namespace RealmCore.Server.Extensions;

public static class IStringLocalizerExtensions
{
    public static string GetOr<T>(this IStringLocalizer<T>? stringLocalizer, string key, string @default, params object[] args)
    {
        if(stringLocalizer == null)
            return string.Format(@default, args);
        var localizer = stringLocalizer.GetString(key);
        return string.Format(localizer.ResourceNotFound ? @default : localizer.Value, args);
    }
}
