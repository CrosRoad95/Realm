namespace RealmCore.Server.Extensions;

public static class LoggerExtensions
{
    public static IDisposable? BeginEntity<T>(this ILogger<T> logger, Entity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        Dictionary<string, object> data = new();

        if (entity.TryGetComponent(out UserComponent userComponent))
            data["userId"] = userComponent.Id;
        if (entity.TryGetComponent(out PlayerElementComponent playerElementComponent))
        {
            if(playerElementComponent.Client.Serial != null)
                data["serial"] = playerElementComponent.Client.Serial;
        }
        if (entity.TryGetComponent(out PrivateVehicleComponent privateVehicleComponent))
            data["vehicleId"] = privateVehicleComponent.Id;

        return logger.BeginScope(data);
    }
    
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
            case Player player:
                data["name"] = player.Name;
                if(player.Client.Serial != null)
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
