using RealmCore.Server.Concepts.Sessions;

namespace RealmCore.Server.Services.Players;

internal sealed class PlayerSessionsService : IPlayerSessionsService, IDisposable
{
    private readonly object _lock = new();
    private readonly List<Session> _sessions = [];
    public RealmPlayer Player { get; }
    public event Action<IPlayerSessionsService, Session>? Started;
    public event Action<IPlayerSessionsService, Session>? Ended;

    public PlayerSessionsService(PlayerContext playerContext)
    {
        Player = playerContext.Player;
    }

    public TSession BeginSession<TSession>(params object[] parameters) where TSession : Session
    {
        TSession session;
        lock (_lock)
        {
            if (_sessions.OfType<TSession>().Any())
                throw new InvalidOperationException();
            session = ActivatorUtilities.CreateInstance<TSession>(Player.ServiceProvider, parameters);
            _sessions.Add(session);
            try
            {
                session.Start();
            }
            catch(Exception)
            {
                _sessions.Remove(session);
                throw;
            }
        }
        Started?.Invoke(this, session);
        return session;
    }

    public bool IsDuringSession<TSession>()
    {
        lock (_lock)
        {
            return _sessions.OfType<TSession>().Any();
        }
    }

    public bool TryGetSession<TSession>(out TSession session)
    {
        lock (_lock)
        {
            session = _sessions.OfType<TSession>().FirstOrDefault();
            return session != null;
        }
    }

    public TSession? GetSession<TSession>()
    {
        lock (_lock)
        {
            return _sessions.OfType<TSession>().FirstOrDefault();
        }
    }
    
    public TSession GetRequiredSession<TSession>()
    {
        lock (_lock)
        {
            return _sessions.OfType<TSession>().First();
        }
    }

    public void EndSession<TSession>() where TSession: Session
    {
        Session session;
        lock (_lock)
        {
            session = _sessions.OfType<TSession>().First();
            try
            {
                session.End();
            }
            finally
            {
                _sessions.Remove(session);
            }
        }
        Ended?.Invoke(this, session);
    }
    
    public void EndSession<TSession>(TSession session) where TSession: Session
    {
        lock (_lock)
        {
            if (!_sessions.Contains(session))
                throw new InvalidOperationException();
            try
            {
                session.End();
            }
            finally
            {
                _sessions.Remove(session);
            }
        }
        Ended?.Invoke(this, session);
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
            foreach (var session in new List<Session>(_sessions))
            {
                try
                {
                    session.End();
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
