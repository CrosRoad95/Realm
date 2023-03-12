namespace Realm.Server.Extensions;

public static class OverlayServiceExtensions
{
    public static string AddRing3dDisplay(this OverlayService overlayService, Entity entity, Vector3 position, TimeSpan time)
    {
        return overlayService.AddRing3dDisplay(entity.GetRequiredComponent<PlayerElementComponent>().Player, position, time);
    }

    public static void RemoveRing3dDisplay(this OverlayService overlayService, Entity entity, string id)
    {
        overlayService.RemoveRing3dDisplay(entity.GetRequiredComponent<PlayerElementComponent>().Player, id);
    }
}
