using Realm.Resources.Nametags;

namespace Realm.Server.Extensions;

public static class NametagsServiceExtensions
{
    public static void SetNametagRenderingEnabled(this INametagsService service, Entity entity, bool enabled)
    {
        var player = entity.GetRequiredComponent<PlayerElementComponent>().Player;
        service.SetNametagRenderingEnabled(player, enabled);
    }

    public static void SetLocalPlayerRenderingEnabled(this INametagsService service, Entity entity, bool enabled)
    {
        var player = entity.GetRequiredComponent<PlayerElementComponent>().Player;
        service.SetLocalPlayerRenderingEnabled(player, enabled);
    }
}
