namespace Realm.Server.ResourcesLogic;

internal class ClientInterfaceLogic
{
    private readonly Resource _resource;
    public ClientInterfaceLogic(IResourceProvider resourceProvider, IRPGServer rpgServer)
    {
        _resource = resourceProvider.GetResource("ClientInterface");
        _resource.AddGlobals();

        rpgServer.PlayerJoined += Start;
    }

    public void Start(Player player)
    {
        _resource.StartFor(player);
    }
}
