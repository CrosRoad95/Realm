namespace Realm.Server.ResourcesLogic;

internal class ClientUILogic : IAutoStartResource
{
    private readonly Resource _resource;
    public ClientUILogic(IResourceProvider resourceProvider, IGuiFilesProvider guiFilesProvider)
    {
        _resource = resourceProvider.GetResource("UI");

        foreach (var pair in guiFilesProvider.GetFiles())
            _resource.NoClientScripts[$"{_resource!.Name}/{pair.Item1}"] = pair.Item2;
    }

    public void StartFor(IRPGPlayer player)
    {
        _resource.StartFor(player as Player); // TODO: Make it better
    }
}
