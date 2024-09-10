using Microsoft.Extensions.Logging;

namespace RealmCore.Resources.Base.Extensions;

public static class LoggerExtensions
{
    public static void ResourceFailedToStart<T>(this ILogger logger, Exception ex) where T: Resource
    {
        logger.LogError(ex, $"Failed to start resource '{typeof(T).Name}'");
    }
}