namespace Realm.Server.ResourcesLogic;

internal class ClientUILogic
{
    private readonly Resource _resource;
    private readonly EventFunctions _eventFunctions;
    private readonly FromLuaValueMapper _fromLuaValueMapper;

    public ClientUILogic(IResourceProvider resourceProvider, IGuiFilesProvider guiFilesProvider, IRPGServer rpgServer, EventFunctions eventFunctions, FromLuaValueMapper fromLuaValueMapper)
    {
        _eventFunctions = eventFunctions;
        _fromLuaValueMapper = fromLuaValueMapper;
        _resource = resourceProvider.GetResource("UI");
        _resource.AddGlobals();

        foreach (var pair in guiFilesProvider.GetFiles())
            _resource.NoClientScripts[$"{_resource!.Name}/{pair.Item1}"] = pair.Item2;

        rpgServer.PlayerJoined += Start;
        rpgServer.AddEventHandler("internalSubmitForm", InternalSubmitFormHandler);
    }

    private async Task<object?> InternalSubmitFormHandler(LuaEvent luaEvent)
    {
        var formContext = new FormContext(luaEvent, _fromLuaValueMapper);
        await _eventFunctions.InvokeEvent("onFormSubmit", formContext);
        return new object[] { formContext.Id, formContext.Name, formContext.IsSuccess, formContext.Response };
    }

    private void Start(Player player)
    {
        var rpgPlayer = player as RPGPlayer;
        _resource.StartFor(player);
        rpgPlayer.ResourceReady += Player_ResourceReady;
    }

    public void SetGuiOpen(RPGPlayer player, string guiName)
    {
        player.TriggerClientEvent("internalUiOpenGui", guiName);
    }
    
    public void SetGuiClose(RPGPlayer player, string guiName)
    {
        player.TriggerClientEvent("internalUiCloseGui", guiName);
    }

    private void Player_ResourceReady(RPGPlayer player, int netId)
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
