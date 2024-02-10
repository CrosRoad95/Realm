namespace RealmCore.Server.Extensions;

public static class LoggerExtensions
{
    public static IDisposable? BeginElement<T>(this ILogger<T> logger, Element element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        Dictionary<string, object> data = new()
        {
            ["elementType"] = element.ElementType
        };

        switch (element)
        {
            case RealmPlayer player:
                data["name"] = player.Name;
                data["serial"] = player.Client.GetSerialOrDefault();
                if(player.PersistentId != null)
                    data["userId"] = player.PersistentId;
                break;
        }

        return logger.BeginScope(data);
    }

    public static void LogHandleError<T>(this ILogger<T> logger, Exception ex, [CallerMemberName] string memberName = "")
    {
        logger.LogError(ex, "An error occurred in function '{memberName}'", memberName);
    }
}
