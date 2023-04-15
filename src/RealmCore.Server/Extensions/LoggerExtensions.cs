namespace RealmCore.Server.Extensions;

public static class LoggerExtensions
{
    public static IDisposable? BeginEntity<T>(this ILogger<T> logger, Entity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        Dictionary<string, object> data = new();
        switch (entity.Tag)
        {
            case EntityTag.Player:
                {

                    if (entity.TryGetComponent(out UserComponent userComponent))
                        data["userId"] = userComponent.Id;
                    if (entity.TryGetComponent(out PlayerElementComponent playerElementComponent))
                        data["serial"] = playerElementComponent.Client.Serial;
                }
                break;
        }
        return logger.BeginScope(data);
    }

    public static void LogHandleError<T>(this ILogger<T> logger, Exception ex, [CallerMemberName] string memberName = "")
    {
        logger.LogError(ex, "An error occurred in function '{memberName}'", memberName);
    }

}
