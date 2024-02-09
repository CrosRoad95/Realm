namespace RealmCore.Server.Modules.Players.Gui;

internal sealed class DxGuiSystemServiceLogic : PlayerLogic
{
    private readonly IGuiSystemService? _guiSystemService;
    private readonly FromLuaValueMapper _fromLuaValueMapper;
    private readonly ILogger<DxGuiSystemServiceLogic> _logger;

    public DxGuiSystemServiceLogic(FromLuaValueMapper fromLuaValueMapper, ILogger<DxGuiSystemServiceLogic> logger, MtaServer mtaServer, IGuiSystemService? guiSystemService = null) : base(mtaServer)
    {
        _fromLuaValueMapper = fromLuaValueMapper;
        _logger = logger;
        if (guiSystemService != null)
        {
            guiSystemService.FormSubmitted += HandleFormSubmitted;
            guiSystemService.ActionExecuted += HandleActionExecuted;
            _guiSystemService = guiSystemService;
        }
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.Gui.Changed += HandleGuiChanged;
    }

    protected override void PlayerLeft(RealmPlayer player)
    {
        player.Gui.Changed -= HandleGuiChanged;
    }

    private void HandleGuiChanged(IPlayerGuiFeature playerGuiService, RealmPlayer player, IPlayerGui? previousGui, IPlayerGui? currentGui)
    {
        if (_guiSystemService == null)
            return;

        if (currentGui is DxGui currentDxGui)
        {
            _guiSystemService.OpenGui(player, currentDxGui.Name, currentDxGui.CursorLess);
        }
        else if (previousGui is DxGui previousDxGui)
        {
            _guiSystemService.CloseGui(player, previousDxGui.Name, previousDxGui.CursorLess);
        }
    }

    private async void HandleActionExecuted(LuaEvent luaEvent)
    {
        if (_guiSystemService == null)
            return;

        try
        {
            var player = (RealmPlayer)luaEvent.Player;
            if (player.Gui.Current is IGuiHandlers guiHandlers)
            {
                var (id, guiName, actionName, data) = luaEvent.Read<string, string, string, LuaValue>(_fromLuaValueMapper);

                try
                {
                    await guiHandlers.HandleAction(new ActionContext(player, actionName, data));
                }
                catch (Exception ex)
                {
                    _logger.LogHandleError(ex);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read luaEvent parameters");
        }
    }

    private async void HandleFormSubmitted(LuaEvent luaEvent)
    {
        if (_guiSystemService == null)
            return;

        try
        {
            var player = (RealmPlayer)luaEvent.Player;
            if (player.Gui.Current is IGuiHandlers guiHandlers)
            {
                var (id, guiName, formName, data) = luaEvent.Read<string, string, string, LuaValue>(_fromLuaValueMapper);

                try
                {
                    var formContext = new FormContext(player, formName, data, _guiSystemService);
                    await guiHandlers.HandleForm(formContext);
                }
                catch (Exception ex)
                {
                    _logger.LogHandleError(ex);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read luaEvent parameters");
        }
    }
}
