namespace RealmCore.Server.Modules.Players.Jobs;

public interface IPlayerSessionsFeature : IPlayerFeature, IEnumerable<Session>
{
    int Count { get; }

    event Action<IPlayerSessionsFeature, Session>? Started;
    event Action<IPlayerSessionsFeature, Session>? Ended;

    TSession Begin<TSession>(params object[] parameters) where TSession : Session;
    Session Begin(Type sessionType, params object[] parameters);
    TSession Get<TSession>();
    bool IsDuring<TSession>();
    bool IsDuring<TSession>(TSession session) where TSession : Session;
    bool IsDuring(Type type);
    bool TryEnd<TSession>() where TSession : Session;
    bool TryEnd<TSession>(TSession session) where TSession : Session;
    bool TryGet<TSession>(out TSession session);
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

    public TSession Begin<TSession>(params object[] parameters) where TSession : Session
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

    public Session Begin(Type sessionType, params object[] parameters)
    {
        Session session;
        lock (_lock)
        {
            if (_sessions.Any(x => x.GetType() == sessionType))
                throw new SessionAlreadyBegunException(sessionType);

            session = (Session)ActivatorUtilities.CreateInstance(Player.ServiceProvider, sessionType, parameters);
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

    public bool IsDuring<TSession>()
    {
        lock (_lock)
            return IsDuringSessionCore<TSession>();
    }

    public bool IsDuring<TSession>(TSession session) where TSession : Session
    {
        lock (_lock)
            return _sessions.Contains(session);
    }
    
    public bool IsDuring(Type type)
    {
        lock (_lock)
            return _sessions.Any(x => x.GetType() == type);
    }

    public bool TryGet<TSession>([NotNullWhen(true)] out TSession outSession)
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

    public TSession Get<TSession>()
    {
        lock (_lock)
        {
            var session = _sessions.OfType<TSession>().FirstOrDefault();
            if(session == null)
                throw new SessionNotFoundException(typeof(TSession));
            return session;
        }
    }

    public bool TryEnd<TSession>() where TSession : Session
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

    public bool TryEnd<TSession>(TSession session) where TSession : Session
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
        Session[] view;
        {
            lock (_lock)
                view = [.. _sessions];
        }

        foreach (var session in view)
        {
            yield return session;
        }
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
