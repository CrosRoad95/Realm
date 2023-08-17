namespace RealmCore.Server.Components.Players;

public abstract class StatefulGuiComponent<TState> : GuiComponent
{
    [Inject]
    private IGuiSystemService GuiSystemService { get; set; } = default!;
    [Inject]
    private LuaValueMapper LuaValueMapper { get; set; } = default!;

    private readonly TState _state;
    private readonly Dictionary<LuaValue, LuaValue> _stateChange = new();

    public StatefulGuiComponent(string name, bool cursorLess, TState initialState)
        : base(name, cursorLess)
    {
        _state = initialState;
    }

    protected virtual void PreGuiOpen(TState state) { }

    protected override void OpenGui()
    {
        PreGuiOpen(_state);
        var playerElementComponent = Entity.GetRequiredComponent<PlayerElementComponent>();
        GuiSystemService.OpenGui(playerElementComponent.Player, _name, _cursorless, LuaValueMapper.UniversalMap(_state));
    }

    private void FlushChanged()
    {
        var playerElementComponent = Entity.GetRequiredComponent<PlayerElementComponent>();
        GuiSystemService.SendStateChanged(playerElementComponent.Player, _name, _stateChange);
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
        if (property.GetValue(_state) != value as object)
        {
            property.SetValue(_state, value);
            _stateChange[memberExpression.Member.Name] = LuaValueMapper.Map(value);
            FlushChanged();
        }
    }

    protected void ChangeState<TValue>(Expression<Func<TState, TValue>> exp, Func<TState, TValue> value)
    {
        if (exp.Body is not MemberExpression memberExpression)
            throw new InvalidExpressionException();

        var property = (PropertyInfo)memberExpression.Member;
        var stateValue = property.GetValue(_state);
        var newValue = value(_state);
        if (stateValue != newValue as object)
        {
            property.SetValue(_state, newValue);
            _stateChange[memberExpression.Member.Name] = LuaValueMapper.Map(newValue);
            FlushChanged();
        }
    }
}
