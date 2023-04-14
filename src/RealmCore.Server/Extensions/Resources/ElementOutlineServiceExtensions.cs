namespace RealmCore.Server.Extensions.Resources;

public static class ElementOutlineServiceExtensions
{
    public static void SetRenderingEnabled(this IElementOutlineService service, Entity playerEntity, bool enabled)
    {
        service.SetRenderingEnabled(playerEntity.Player, enabled);
    }

    public static void SetEntityOutlineForPlayer(this IElementOutlineService service, Entity playerEntity, Entity elementEntity, Color color)
    {
        service.SetElementOutlineForPlayer(playerEntity.Player, elementEntity.Element, color);
    }

    public static void RemoveEntityOutlineForPlayer(this IElementOutlineService service, Entity playerEntity)
    {
        service.RemoveElementOutlineForPlayer(playerEntity.Player, playerEntity.Element);
    }
}
