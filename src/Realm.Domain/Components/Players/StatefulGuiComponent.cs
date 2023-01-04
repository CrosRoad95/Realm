using System.Data;

namespace Realm.Domain.Components.Players;

public abstract class StatefulGuiComponent<TState> : GuiComponent
{
    [Inject]
    private AgnosticGuiSystemService AgnosticGuiSystemService { get; set; } = default!;
    [Inject]
    private LuaValueMapper LuaValueMapper { get; set; } = default!;

    private readonly TState _state;
    private readonly Dictionary<LuaValue, LuaValue> _stateChange = new();
    public StatefulGuiComponent(string name, bool cursorless, TState initialState)
        : base(name, cursorless)
    {
        _state = initialState;
    }

    protected override async Task OpenGui()
    {
        var playerElementComponent = Entity.GetRequiredComponent<PlayerElementComponent>();
        await AgnosticGuiSystemService.OpenGui(playerElementComponent.Player, _name, _cursorless, LuaValueMapper.UniversalMap(_state));
    }

    private void FlushChanged()
    {
        var playerElementComponent = Entity.GetRequiredComponent<PlayerElementComponent>();
        AgnosticGuiSystemService.SendStateChanged(playerElementComponent.Player, _name, _stateChange);
        _stateChange.Clear();
    }

    protected TValue? GetStateValue<TValue>(Expression<Func<TState, TValue>> exp)
    {
        if (exp.Body is not MemberExpression memberExpression)
            throw new InvalidExpressionException();

        var property = (PropertyInfo)memberExpression.Member;
        return (TValue?)property.GetValue(_state);
    }

    protected void ChangeState<TValue>(Expression<Func<TState, TValue>> exp, TValue value)
    {
        if (exp.Body is not MemberExpression memberExpression)
            throw new InvalidExpressionException();

        var property = (PropertyInfo)memberExpression.Member;
        if(property.GetValue(_state) != value as object)
        {
            property.SetValue(_state, value);
            _stateChange[memberExpression.Member.Name] = LuaValueMapper.Map(value);
            FlushChanged();
        }
    }
}
