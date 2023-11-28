using RealmCore.Server.Concepts.Sessions;

namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerSessionsService : IPlayerService, IEnumerable<Session>
{
    event Action<IPlayerSessionsService, Session>? Started;
    event Action<IPlayerSessionsService, Session>? Ended;

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
