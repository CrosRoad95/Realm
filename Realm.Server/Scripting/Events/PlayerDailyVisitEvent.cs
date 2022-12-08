using Realm.Domain.Elements;

namespace Realm.Server.Scripting.Events;

[NoDefaultScriptAccess]
public class PlayerDailyVisitEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly RPGPlayer _player;
    private readonly int _visitsInRow;
    private readonly bool _reseted;

    public static string EventName => "onPlayerDailyVisit";

    [ScriptMember("player")]
    public RPGPlayer Player
    {
        get
        {
            CheckIfDisposed();
            return _player;
        }
    }

    [ScriptMember("visitsInRow")]
    public int VisitsInRow
    {
        get
        {
            CheckIfDisposed();
            return _visitsInRow;
        }
    }
    

    [ScriptMember("reseted")]
    public bool Reseted
    {
        get
        {
            CheckIfDisposed();
            return _reseted;
        }
    }

    public PlayerDailyVisitEvent(RPGPlayer player, int visitsInRow, bool reseted)
    {
        _player = player;
        _visitsInRow = visitsInRow;
        _reseted = reseted;
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
