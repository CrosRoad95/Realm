namespace RealmCore.Server.Extensions.Resources;

public static class ClientInterfaceServiceExtensions
{
    public static void SetClipboard(this IClientInterfaceService clientInterfaceService, Entity entity, string content)
    {
        clientInterfaceService.SetClipboard(entity.GetPlayer(), content);
    }
}
