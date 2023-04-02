using Realm.Resources.Nametags;

namespace Realm.Server.Extensions;

public static class NametagsServiceExtensions
{
    public static void SetNametagRenderingEnabled(this INametagsService service, Entity entity, bool enabled)
    {
        service.SetNametagRenderingEnabled(entity.Player, enabled);
    }

    public static void SetLocalPlayerRenderingEnabled(this INametagsService service, Entity entity, bool enabled)
    {
        service.SetLocalPlayerRenderingEnabled(entity.Player, enabled);
    }
}
