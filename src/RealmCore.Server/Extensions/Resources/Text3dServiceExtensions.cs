namespace RealmCore.Server.Extensions.Resources;

public static class Text3dServiceExtensions
{
    public static void SetRenderingEnabled(this Text3dService text3dService, Entity entity, bool enabled)
    {
        text3dService.SetRenderingEnabled(entity.GetRequiredComponent<PlayerElementComponent>(), enabled);
    }
}
