namespace RealmCore.Server.Logic.Components;

internal sealed class StatefulGuiComponentBaseLogic : ComponentLogic<StatefulDxGuiComponentBase>
{
    private readonly IEntityEngine _entityEngine;
    private readonly IGuiSystemService _guiSystemService;
    private readonly LuaValueMapper _luaValueMapper;

    public StatefulGuiComponentBaseLogic(IEntityEngine entityEngine, IGuiSystemService guiSystemService, LuaValueMapper luaValueMapper) : base(entityEngine)
    {
        _entityEngine = entityEngine;
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
        _guiSystemService.OpenGui(statefulGuiComponentBase.Entity.GetRequiredComponent<PlayerElementComponent>(), name, cursorLess, _luaValueMapper.UniversalMap(state));
    }
    private void HandleStateChanged(StatefulDxGuiComponentBase statefulGuiComponentBase, string name, Dictionary<LuaValue, object?> state)
    {
        var newState = state.ToDictionary(x => x.Key, y => _luaValueMapper.Map(y.Value));
        _guiSystemService.SendStateChanged(statefulGuiComponentBase.Entity.GetRequiredComponent<PlayerElementComponent>(), name, newState);
    }
}
