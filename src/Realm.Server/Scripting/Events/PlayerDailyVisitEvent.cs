namespace Realm.Server.Scripting.Events;

[NoDefaultScriptAccess]
public class PlayerDailyVisitEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly RPGPlayer _rpgPlayer;
    private readonly int _visitsInRow;
    private readonly bool _reseted;

    public static string EventName => "onPlayerDailyVisit";

    [ScriptMember("player")]
    public RPGPlayer RPGPlayer
    {
        get
        {
            CheckIfDisposed();
            return _rpgPlayer;
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

    public PlayerDailyVisitEvent(RPGPlayer rpgPlayer, int visitsInRow, bool reseted)
    {
        _rpgPlayer = rpgPlayer;
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
