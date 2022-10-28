namespace Realm.Server.Scripting.Events;

public class FormContext
{
    private readonly LuaEvent _luaEvent;
    private readonly FromLuaValueMapper _fromLuaValueMapper;

    public RPGPlayer Player => (RPGPlayer)_luaEvent.Player;
    public string Id => _luaEvent.Parameters[0].StringValue ?? throw new InvalidOperationException();
    public string Name => _luaEvent.Parameters[1].StringValue ?? throw new InvalidOperationException();

    [NoScriptAccess]
    public bool IsSuccess { get; private set; } = false;
    [NoScriptAccess]
    public string? Response { get; private set; } = null;

    public PropertyBag Form
    {
        get
        {
            var bag = new PropertyBag();
            foreach (var pair in _luaEvent.Parameters[2].TableValue)
                bag.Add(_fromLuaValueMapper.Map(typeof(string), pair.Key) as string, _fromLuaValueMapper.Map(typeof(string), pair.Value));
            return bag;
        }
    }

    public FormContext(LuaEvent luaEvent, FromLuaValueMapper fromLuaValueMapper)
    {
        _luaEvent = luaEvent;
        _fromLuaValueMapper = fromLuaValueMapper;
    }

    public void Success()
    {
        IsSuccess = true;
    }

    public void Success(string response)
    {
        IsSuccess = true;
        Response = response; 
    }

    public void Error(string errorMessage)
    {
        IsSuccess = false;
        Response = errorMessage; 
    }
}
