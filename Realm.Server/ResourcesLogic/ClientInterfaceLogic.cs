namespace Realm.Server.ResourcesLogic;

internal class ClientInterfaceLogic
{
    private readonly Resource _resource;
    public ClientInterfaceLogic(IResourceProvider resourceProvider)
    {
        _resource = resourceProvider.GetResource("ClientInterface");
        _resource.AddGlobals();
    }

    public void StartFor(RPGPlayer player)
    {
        _resource.StartFor(player);
    }
}
