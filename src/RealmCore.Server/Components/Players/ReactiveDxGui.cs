namespace RealmCore.Server.Components.Players;

public abstract class ReactiveDxGui : DxGui
{
    public Action<ReactiveDxGui, string, bool, object?>? GuiOpened;
    public Action<ReactiveDxGui, string, Dictionary<LuaValue, object?>>? StateChanged;

    public ReactiveDxGui(RealmPlayer player, string name, bool cursorLess)
        : base(player, name, cursorLess)
    {

    }

    protected void RelayGuiOpened(ReactiveDxGui statefulGuiComponentBase, string name, bool cursorLess, object? state)
    {
        GuiOpened?.Invoke(statefulGuiComponentBase, name, cursorLess, state);
    }
}

public abstract class ReactiveDxGui<TState> : ReactiveDxGui
{
    private readonly TState _state;
    private readonly Dictionary<LuaValue, object?> _stateChange = [];

    public ReactiveDxGui(RealmPlayer player, string name, bool cursorLess, TState initialState)
        : base(player, name, cursorLess)
    {
        _state = initialState;
    }

    protected virtual void PreGuiOpen(TState state) { }

    protected void OpenGui()
    {
        PreGuiOpen(_state);
        GuiOpened?.Invoke(this, _name, _cursorLess, _state);
    }

    private void FlushChanged()
    {
        StateChanged?.Invoke(this, _name, _stateChange);
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
            _stateChange[memberExpression.Member.Name] = value;
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
            _stateChange[memberExpression.Member.Name] = newValue;
            FlushChanged();
        }
    }
}
