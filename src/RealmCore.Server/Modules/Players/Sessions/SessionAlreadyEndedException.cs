namespace RealmCore.Server.Modules.Players.Sessions;

public class SessionAlreadyEndedException : Exception
{
    public Session Session { get; }

    public SessionAlreadyEndedException(Session session)
    {
        Session = session;
    }
}
