namespace Realm.Server.ResourcesLogic;

internal class ClientUILogic : IAutoStartResource
{
    private readonly Resource _resource;

    public ClientUILogic(IResourceProvider resourceProvider, IGuiFilesProvider guiFilesProvider)
    {
        _resource = resourceProvider.GetResource("UI");
        _resource.AddGlobals();

        foreach (var pair in guiFilesProvider.GetFiles())
            _resource.NoClientScripts[$"{_resource!.Name}/{pair.Item1}"] = pair.Item2;
    }

    public void StartFor(IRPGPlayer player)
    {
        _resource.StartFor(player as Player); // TODO: Make it better
        player.ResourceReady += Player_ResourceReady;
    }

    public void SetGuiOpen(IRPGPlayer player, string guiName)
    {
        player.TriggerClientEvent("internalUiOpenGui", guiName);
    }
    
    public void SetGuiClose(IRPGPlayer player, string guiName)
    {
        player.TriggerClientEvent("internalUiCloseGui", guiName);
    }

    private void Player_ResourceReady(IRPGPlayer player, int netId)
    {
        if(_resource.NetId != netId)
            return;

        SetGuiOpen(player, "login");
        player.TriggerClientEvent("internalUiStatechanged", new List<object>
        {
            "login",
            "asd",
            "dsa",
        });

        //Task.Run(async () =>
        //{
        //    await Task.Delay(1000);
        //    SetGuiClose(player, "login");
        //});
    }
}
