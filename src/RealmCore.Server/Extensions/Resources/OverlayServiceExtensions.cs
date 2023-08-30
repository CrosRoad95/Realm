namespace RealmCore.Server.Extensions.Resources;

public static class OverlayServiceExtensions
{
    public static void AddNotification(this IOverlayService overlayService, Entity entity, string message)
    {
        overlayService.AddNotification(entity.GetPlayer(), message);
    }
}
