﻿namespace RealmCore.Server.Modules.Players.Jobs;

public interface IPlayerSessionsFeature : IPlayerFeature, IEnumerable<Session>
{
    event Action<IPlayerSessionsFeature, Session>? Started;
    event Action<IPlayerSessionsFeature, Session>? Ended;

    TSession BeginSession<TSession>(params object[] parameters) where TSession : Session;
    void EndSession<TSession>() where TSession : Session;
    void EndSession<TSession>(TSession session) where TSession : Session;
    TSession GetRequiredSession<TSession>();
    TSession? GetSession<TSession>();
    bool IsDuringSession<TSession>();
    void TryEndSession<TSession>() where TSession : Session;
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
                throw new InvalidOperationException();
            session = ActivatorUtilities.CreateInstance<TSession>(Player.ServiceProvider, parameters);
            _sessions.Add(session);
            try
            {
                session.Start();
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

    public bool IsDuringSession<TSession>()
    {
        lock (_lock)
        {
            return _sessions.OfType<TSession>().Any();
        }
    }

    public bool TryGetSession<TSession>(out TSession outSession)
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

    public void EndSession<TSession>() where TSession : Session
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

    public void EndSession<TSession>(TSession session) where TSession : Session
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

    public void TryEndSession<TSession>() where TSession : Session
    {
        Session session;
        bool ended = false;
        lock (_lock)
        {
            session = _sessions.OfType<TSession>().First();
            try
            {
                session.End();
            }
            finally
            {
                ended = _sessions.Remove(session);
            }
        }
        if (ended)
            Ended?.Invoke(this, session);
    }

    public bool TryEndSession<TSession>(TSession session) where TSession : Session
    {
        lock (_lock)
        {
            if (!_sessions.Contains(session))
                return false;
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