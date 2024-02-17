using RealmCore.Server.Modules.Players.Sessions;
using System.Diagnostics.Contracts;

namespace RealmCore.Server.Modules.Players.Jobs;

public interface IPlayerSessionsFeature : IPlayerFeature, IEnumerable<Session>
{
    int Count { get; }

    event Action<IPlayerSessionsFeature, Session>? Started;
    event Action<IPlayerSessionsFeature, Session>? Ended;

    TSession BeginSession<TSession>(params object[] parameters) where TSession : Session;
    void EndSession<TSession>() where TSession : Session;
    void EndSession<TSession>(TSession session) where TSession : Session;
    TSession GetSession<TSession>();
    bool IsDuringSession<TSession>();
    bool IsDuringSession<TSession>(TSession session) where TSession : Session;
    bool TryEndSession<TSession>() where TSession : Session;
    bool TryEndSession<TSession>(TSession session) where TSession : Session;
    bool TryGetSession<TSession>(out TSession session);
}

internal sealed class PlayerSessionsFeature : IPlayerSessionsFeature, IDisposable
{
    private readonly object _lock = new();
    private readonly List<Session> _sessions = [];
    public event Action<IPlayerSessionsFeature, Session>? Started;
    public event Action<IPlayerSessionsFeature, Session>? Ended;

    public RealmPlayer Player { get; init; }

    public int Count
    {
        get
        {
            lock (_lock)
                return _sessions.Count;
        }
    }

    public PlayerSessionsFeature(PlayerContext playerContext)
    {
        Player = playerContext.Player;
    }

    public TSession BeginSession<TSession>(params object[] parameters) where TSession : Session
    {
        TSession session;
        lock (_lock)
        {
            if (_sessions.OfType<TSession>().Any())
                throw new SessionAlreadyBegunException(typeof(TSession));

            session = ActivatorUtilities.CreateInstance<TSession>(Player.ServiceProvider, parameters);
            _sessions.Add(session);
            try
            {
                session.TryStart();
            }
            catch (Exception)
            {
                _sessions.Remove(session);
                throw;
            }
        }
        Started?.Invoke(this, session);
        return session;
    }

    private bool IsDuringSessionCore<TSession>() => _sessions.OfType<TSession>().Any();

    public bool IsDuringSession<TSession>()
    {
        lock (_lock)
            return IsDuringSessionCore<TSession>();
    }

    public bool IsDuringSession<TSession>(TSession session) where TSession : Session
    {
        lock (_lock)
            return _sessions.Contains(session);
    }

    public bool TryGetSession<TSession>([NotNullWhen(true)] out TSession outSession)
    {
        lock (_lock)
        {
            foreach (var session in _sessions)
            {
                if (session is TSession foundSession)
                {
                    outSession = foundSession;
                    return true;
                }
            }

            outSession = default!;
            return false;
        }
    }

    public TSession GetSession<TSession>()
    {
        lock (_lock)
        {
            var session = _sessions.OfType<TSession>().FirstOrDefault();
            if(session == null)
                throw new SessionNotFoundException(typeof(TSession));
            return session;
        }
    }

    public void EndSession<TSession>() where TSession : Session
    {
        Session? session;
        lock (_lock)
        {
            session = _sessions.OfType<TSession>().FirstOrDefault();
            if (session == null)
                throw new SessionNotFoundException(typeof(TSession));
            try
            {
                session.Dispose();
            }
            finally
            {
                _sessions.Remove(session);
                Ended?.Invoke(this, session);
            }
        }
    }

    public void EndSession<TSession>(TSession session) where TSession : Session
    {
        lock (_lock)
        {
            if (!_sessions.Contains(session))
                throw new SessionNotFoundException(typeof(TSession));

            try
            {
                session.Dispose();
            }
            finally
            {
                _sessions.Remove(session);
                Ended?.Invoke(this, session);
            }
        }
    }

    public bool TryEndSession<TSession>() where TSession : Session
    {
        Session? session;
        lock (_lock)
        {
            session = _sessions.OfType<TSession>().FirstOrDefault();
            if (session == null)
                return false;
            try
            {
                session.Dispose();
            }
            finally
            {
                _sessions.Remove(session);
                Ended?.Invoke(this, session);
            }
        }
        return true;
    }

    public bool TryEndSession<TSession>(TSession session) where TSession : Session
    {
        lock (_lock)
        {
            if (!_sessions.Contains(session))
                return false;
            try
            {
                session.Dispose();
            }
            finally
            {
                _sessions.Remove(session);
                Ended?.Invoke(this, session);
            }
        }
        return true;
    }

    public IEnumerator<Session> GetEnumerator()
    {
        lock (_lock)
            return new List<Session>(_sessions).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        lock (_lock)
        {
            foreach (var session in this)
            {
                try
                {
                    session.Dispose();
                    Ended?.Invoke(this, session);
                }
                catch (Exception) { }
                finally
                {
                    _sessions.Remove(session);
                }
            }
        }
    }
}
