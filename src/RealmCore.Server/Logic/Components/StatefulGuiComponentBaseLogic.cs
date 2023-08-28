using Newtonsoft.Json.Linq;
using RealmCore.ECS;
using RealmCore.Resources.GuiSystem;
using RealmCore.Server.Components.Elements;

namespace RealmCore.Server.Logic.Components;

internal sealed class StatefulGuiComponentBaseLogic : ComponentLogic<StatefulGuiComponentBase>
{
    private readonly IEntityEngine _ecs;
    private readonly IGuiSystemService _guiSystemService;
    private readonly LuaValueMapper _luaValueMapper;

    public StatefulGuiComponentBaseLogic(IEntityEngine ecs, IGuiSystemService guiSystemService, LuaValueMapper luaValueMapper) : base(ecs)
    {
        _ecs = ecs;
        _guiSystemService = guiSystemService;
        _luaValueMapper = luaValueMapper;
    }

    protected override void ComponentAdded(StatefulGuiComponentBase statefulGuiComponentBase)
    {
        statefulGuiComponentBase.GuiOpened = HandleGuiOpened;
        statefulGuiComponentBase.StateChanged = HandleStateChanged;
    }

    private void HandleGuiOpened(StatefulGuiComponentBase statefulGuiComponentBase, string name, bool cursorLess, object? state)
    {
        _guiSystemService.OpenGui(statefulGuiComponentBase.Entity.GetPlayer(), name, cursorLess, _luaValueMapper.UniversalMap(state));
    }
    private void HandleStateChanged(StatefulGuiComponentBase statefulGuiComponentBase, string name, Dictionary<LuaValue, object?> state)
    {
        var newState = state.ToDictionary(x => x.Key, y => _luaValueMapper.Map(y.Value));
        _guiSystemService.SendStateChanged(statefulGuiComponentBase.Entity.GetPlayer(), name, newState);
    }
}
