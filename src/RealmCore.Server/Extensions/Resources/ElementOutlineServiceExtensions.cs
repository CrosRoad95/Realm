using RealmCore.ECS;

namespace RealmCore.Server.Extensions.Resources;

public static class ElementOutlineServiceExtensions
{
    public static void SetRenderingEnabled(this IElementOutlineService service, Entity playerEntity, bool enabled)
    {
        service.SetRenderingEnabled(playerEntity.GetPlayer(), enabled);
    }

    public static void SetEntityOutlineForPlayer(this IElementOutlineService service, Entity playerEntity, Entity elementEntity, Color color)
    {
        service.SetElementOutlineForPlayer(playerEntity.GetPlayer(), elementEntity.GetElement(), color);
    }

    public static void RemoveEntityOutlineForPlayer(this IElementOutlineService service, Entity playerEntity, Entity elementEntity)
    {
        service.RemoveElementOutlineForPlayer(playerEntity.GetPlayer(), elementEntity.GetElement());
    }
}
