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
        rpgServer.AddEventHandler("internalRequestGuiClose", InternalRequestGuiClose);
    }

    private async Task<object?> InternalSubmitFormHandler(LuaEvent luaEvent)
    {
        var formContext = new FormContext(luaEvent, _fromLuaValueMapper);
        await _eventFunctions.InvokeEvent(formContext);
        return new object?[] { formContext.Id, formContext.EventName, formContext.IsSuccess, formContext.Response };
    }

    private Task<object?> InternalRequestGuiClose(LuaEvent luaEvent)
    {
        string? guiName = _fromLuaValueMapper.Map(typeof(string), luaEvent.Parameters[1]) as string;
        if (guiName == null)
            throw new NullReferenceException(nameof(guiName));

        var player = (RPGPlayer)luaEvent.Player;
        player.OpenGui(guiName);
        return Task.FromResult<object?>(null);
    }

    private void Start(Player player)
    {
        var rpgPlayer = (RPGPlayer)player;
        _resource.StartFor(rpgPlayer);
    }
}
