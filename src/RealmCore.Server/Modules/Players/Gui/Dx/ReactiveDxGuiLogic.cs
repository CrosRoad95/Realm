namespace RealmCore.Server.Modules.Players.Gui.Dx;

internal sealed class ReactiveDxGuiLogic : PlayerLogic
{
    private readonly IGuiSystemService _guiSystemService;
    private readonly LuaValueMapper _luaValueMapper;

    public ReactiveDxGuiLogic(MtaServer server, IGuiSystemService guiSystemService, LuaValueMapper luaValueMapper) : base(server)
    {
        _guiSystemService = guiSystemService;
        _luaValueMapper = luaValueMapper;
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.Gui.Changed += HandleGuiChanged;
    }

    protected override void PlayerLeft(RealmPlayer player)
    {
        player.Gui.Changed -= HandleGuiChanged;
    }

    private void HandleGuiChanged(IPlayerGuiFeature arg1, RealmPlayer arg2, IPlayerGui? arg3, IPlayerGui? currentGui)
    {
        if (currentGui is ReactiveDxGui reactiveDxGui)
        {
            reactiveDxGui.GuiOpened = HandleGuiOpened;
            reactiveDxGui.StateChanged = HandleStateChanged;
        }
    }

    private void HandleGuiOpened(ReactiveDxGui reactiveDxGui, string name, bool cursorLess, object? state)
    {
        _guiSystemService.OpenGui(reactiveDxGui.Player, name, cursorLess, _luaValueMapper.UniversalMap(state));
    }

    private void HandleStateChanged(ReactiveDxGui reactiveDxGui, string name, Dictionary<LuaValue, object?> state)
    {
        var newState = state.ToDictionary(x => x.Key, y => _luaValueMapper.Map(y.Value));
        _guiSystemService.SendStateChanged(reactiveDxGui.Player, name, newState);
    }
}
