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
            case EntityTag.Vehicle:
                {

                    if (entity.TryGetComponent(out PrivateVehicleComponent privateVehicleComponent))
                        data["vehicleId"] = privateVehicleComponent.Id;
                }
                break;
        }
        return logger.BeginScope(data);
    }
    
    public static IDisposable? BeginPlayer<T>(this ILogger<T> logger, Element element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        Dictionary<string, object> data = new()
        {
            ["elementType"] = element.ElementType
        };

        switch (element)
        {
            case Player player:
                data["name"] = player.Name;
                data["serial"] = player.Client.Serial;
                break;
        }
        return logger.BeginScope(data);
    }

    public static void LogHandleError<T>(this ILogger<T> logger, Exception ex, [CallerMemberName] string memberName = "")
    {
        logger.LogError(ex, "An error occurred in function '{memberName}'", memberName);
    }

}
