namespace Realm.Server.ResourcesLogic;

internal class ClientInterfaceLogic : IAutoStartResource
{
    private readonly Resource _resource;
    public ClientInterfaceLogic(IResourceProvider resourceProvider)
    {
        _resource = resourceProvider.GetResource("ClientInterface");
    }

    public void StartFor(IRPGPlayer player)
    {
        _resource.StartFor(player as Player); // TODO: Make it better
    }
}
