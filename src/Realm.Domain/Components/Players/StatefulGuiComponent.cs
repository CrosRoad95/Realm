using Realm.Domain.Components.Elements;
using Realm.Domain.Contextes;
using Realm.Resources.AgnosticGuiSystem;
using SlipeServer.Server.Mappers;
using Realm.Common.Extensions;
using Realm.Domain.Extensions;
using System.Linq.Expressions;
using SlipeServer.Packets.Definitions.Lua;
using System.Reflection;

namespace Realm.Domain.Components.Players;

public abstract class StatefulGuiComponent<TState> : GuiComponent
{
    private readonly TState _state;
    private readonly Dictionary<LuaValue, LuaValue> _stateChange = new();
    public StatefulGuiComponent(string name, bool cursorless, TState initialState)
        : base(name, cursorless)
    {
        _state = initialState;
    }

    protected override async Task OpenGui()
    {
        var agnosticGuiSystemService = Entity.GetRequiredService<AgnosticGuiSystemService>();
        var luaValueMapper = Entity.GetRequiredService<LuaValueMapper>();
        var playerElementComponent = Entity.GetRequiredComponent<PlayerElementComponent>();
        await agnosticGuiSystemService.OpenGui(playerElementComponent.Player, _name, _cursorless, luaValueMapper.UniversalMap(_state));
    }

    private void FlushChanged()
    {
        var playerElementComponent = Entity.GetRequiredComponent<PlayerElementComponent>();
        var agnosticGuiSystemService = Entity.GetRequiredService<AgnosticGuiSystemService>();
        agnosticGuiSystemService.SendStateChanged(playerElementComponent.Player, _name, _stateChange);
        _stateChange.Clear();
    }

    protected TValue GetStateValue<TValue>(Expression<Func<TState, TValue>> exp)
    {
        if (exp.Body is not MemberExpression memberExpression)
            throw new Exception();
        var property = (PropertyInfo)memberExpression.Member;
        return (TValue)property.GetValue(_state);
    }

    protected void ChangeState<TValue>(Expression<Func<TState, TValue>> exp, TValue value)
    {
        var luaValueMapper = Entity.GetRequiredService<LuaValueMapper>();

        if (exp.Body is not MemberExpression memberExpression)
            throw new Exception();
        var property = (PropertyInfo)memberExpression.Member;
        if(property.GetValue(_state) != value as object)
        {
            property.SetValue(_state, value);
            _stateChange[memberExpression.Member.Name] = luaValueMapper.Map(value);
            FlushChanged();
        }
    }
}
