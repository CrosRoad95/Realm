namespace Realm.Server.Resources.ClientInterface;

internal class ClientInterfaceLogic : IClientInterface, IAutoStartResource
{
    public ClientInterfaceLogic()
    {

    }
    
    public void StartFor(IResourceProvider resourceProvider, Player player)
    {
        var resource = resourceProvider.GetResource(ClientInterfaceResource.ResourceName);
        resource.StartFor(player);
    }
}
