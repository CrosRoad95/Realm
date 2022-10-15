namespace Realm.Server.ResourcesLogic;

internal class ClientUILogic : IAutoStartResource
{
    private readonly Resource _resource;
    private readonly IRPGServer _server;

    public ClientUILogic(IResourceProvider resourceProvider, IGuiFilesProvider guiFilesProvider, IRPGServer server)
    {
        _server = server;
        _resource = resourceProvider.GetResource("UI");
        _resource.AddGlobals();

        foreach (var pair in guiFilesProvider.GetFiles())
            _resource.NoClientScripts[$"{_resource!.Name}/{pair.Item1}"] = pair.Item2;

        _server.SubscribeLuaEvent("internalSubmitForm", HandleForSubmissions);
    }

    private Dictionary<TKey, TValue> ConvertDictionary<TKey, TValue>(object? obj)
        where TKey: class
        where TValue : class
    {
        return (obj as Dictionary<object, object>)
            .ToDictionary(kv => kv.Key as TKey, kv => kv.Value as TValue);
    }

    public async Task HandleForSubmissions(ILuaEventContext context)
    {
        var plr = context.Player;
        var name = context.GetValue<string>(1) as string;
        var data = ConvertDictionary<string, string>(context.GetValue<Dictionary<string, string>>(2));

        await Task.Delay(2000);
        context.Response(true, "data...");
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
