namespace Realm.Server.Extensions.Resources;

public static class ElementOutlineServiceExtensions
{
    public static void SetRenderingEnabled(this IElementOutlineService service, Entity entity, bool enabled)
    {
        service.SetRenderingEnabled(entity.Player, enabled);
    }

    public static void SetEntityOutlineForPlayer(this IElementOutlineService service, Entity playerEntity, Entity elementEntity, Color color)
    {
        service.SetElementOutlineForPlayer(playerEntity.Player, elementEntity.Element, color);
    }

    public static void RemoveEntityOutlineForPlayer(this IElementOutlineService service, Entity entity)
    {
        service.RemoveElementOutlineForPlayer(entity.Player, entity.Element);
    }
}
