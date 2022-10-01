namespace Realm.Server.Resources.UI;

internal class ClientUILogic : IClientUI, IAutoStartResource
{
    public ClientUILogic()
    {

    }

    public void StartFor(IResourceProvider resourceProvider, Player player)
    {
        var resource = resourceProvider.GetResource(ClientUIResource.ResourceName);
        resource.StartFor(player);
    }
}
