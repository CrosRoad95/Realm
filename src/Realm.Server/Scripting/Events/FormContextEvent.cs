namespace Realm.Server.Scripting.Events;

public class FormContextEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly LuaEvent _luaEvent;
    private readonly FromLuaValueMapper _fromLuaValueMapper;

    public static string EventName => "onFormSubmit";

    public RPGPlayer rpgPlayer
    {
        get
        {
            CheckIfDisposed();
            return (RPGPlayer)_luaEvent.Player;
        }
    }
    public string Id
    {
        get
        {
            CheckIfDisposed();
            return _luaEvent.Parameters[0].StringValue ?? throw new InvalidOperationException();
        }
    }
    public string Name
    {
        get
        {
            CheckIfDisposed();
            return _luaEvent.Parameters[1].StringValue ?? throw new InvalidOperationException();
        }
    }

    [NoScriptAccess]
    public bool IsSuccess { get; private set; } = false;
    [NoScriptAccess]
    public string? Response { get; private set; } = null;

    public PropertyBag Form
    {
        get
        {
            CheckIfDisposed();
            var bag = new PropertyBag();
            foreach (var pair in _luaEvent.Parameters[2].TableValue ?? throw new InvalidOperationException())
                bag.Add(_fromLuaValueMapper.Map(typeof(string), pair.Key) as string, _fromLuaValueMapper.Map(typeof(string), pair.Value));
            return bag;
        }
    }

    public FormContextEvent(LuaEvent luaEvent, FromLuaValueMapper fromLuaValueMapper)
    {
        _luaEvent = luaEvent;
        _fromLuaValueMapper = fromLuaValueMapper;
    }

    public void Success()
    {
        CheckIfDisposed();
        IsSuccess = true;
    }

    public void Success(string response)
    {
        CheckIfDisposed();
        IsSuccess = true;
        Response = response;
    }

    public void Error(string errorMessage)
    {
        CheckIfDisposed();
        IsSuccess = false;
        Response = errorMessage;
    }

    private void CheckIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    [NoScriptAccess]
    public void Dispose()
    {
        _disposed = true;
    }
}
