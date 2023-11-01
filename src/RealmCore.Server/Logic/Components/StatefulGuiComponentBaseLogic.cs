namespace RealmCore.Server.Logic.Components;

internal sealed class StatefulGuiComponentBaseLogic : ComponentLogic<StatefulDxGuiComponentBase>
{
    private readonly IElementFactory _elementFactory;
    private readonly IGuiSystemService _guiSystemService;
    private readonly LuaValueMapper _luaValueMapper;

    public StatefulGuiComponentBaseLogic(IElementFactory elementFactory, IGuiSystemService guiSystemService, LuaValueMapper luaValueMapper) : base(elementFactory)
    {
        _elementFactory = elementFactory;
        _guiSystemService = guiSystemService;
        _luaValueMapper = luaValueMapper;
    }

    protected override void ComponentAdded(StatefulDxGuiComponentBase statefulGuiComponentBase)
    {
        statefulGuiComponentBase.GuiOpened = HandleGuiOpened;
        statefulGuiComponentBase.StateChanged = HandleStateChanged;
    }

    private void HandleGuiOpened(StatefulDxGuiComponentBase statefulGuiComponentBase, string name, bool cursorLess, object? state)
    {
        _guiSystemService.OpenGui((RealmPlayer)statefulGuiComponentBase.Element, name, cursorLess, _luaValueMapper.UniversalMap(state));
    }
    private void HandleStateChanged(StatefulDxGuiComponentBase statefulGuiComponentBase, string name, Dictionary<LuaValue, object?> state)
    {
        var newState = state.ToDictionary(x => x.Key, y => _luaValueMapper.Map(y.Value));
        _guiSystemService.SendStateChanged((RealmPlayer)statefulGuiComponentBase.Element, name, newState);
    }
}
