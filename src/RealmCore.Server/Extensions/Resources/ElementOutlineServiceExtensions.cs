namespace RealmCore.Server.Extensions.Resources;

public static class ElementOutlineServiceExtensions
{
    public static void SetRenderingEnabled(this IElementOutlineService service, Entity playerEntity, bool enabled)
    {
        service.SetRenderingEnabled(playerEntity.GetRequiredComponent<PlayerElementComponent>(), enabled);
    }

    public static void SetEntityOutlineForPlayer(this IElementOutlineService service, Entity playerEntity, Entity elementEntity, Color color)
    {
        service.SetElementOutlineForPlayer(playerEntity.GetRequiredComponent<PlayerElementComponent>(), elementEntity.GetElement(), color);
    }

    public static void RemoveEntityOutlineForPlayer(this IElementOutlineService service, Entity playerEntity, Entity elementEntity)
    {
        service.RemoveElementOutlineForPlayer(playerEntity.GetRequiredComponent<PlayerElementComponent>(), elementEntity.GetElement());
    }
}
