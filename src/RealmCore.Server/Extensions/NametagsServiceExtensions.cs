namespace RealmCore.Server.Extensions;

public static class NametagsServiceExtensions
{
    public static void SetNametagRenderingEnabled(this INametagsService service, Entity entity, bool enabled)
    {
        service.SetNametagRenderingEnabled(entity.GetPlayer(), enabled);
    }

    public static void SetLocalPlayerRenderingEnabled(this INametagsService service, Entity entity, bool enabled)
    {
        service.SetLocalPlayerRenderingEnabled(entity.GetPlayer(), enabled);
    }
}
