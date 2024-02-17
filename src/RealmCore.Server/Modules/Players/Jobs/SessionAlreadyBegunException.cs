namespace RealmCore.Server.Modules.Players.Jobs;

public class SessionAlreadyBegunException : Exception
{
    public Type SessionType { get; }

    public SessionAlreadyBegunException(Type sessionType) : base("Session already started")
    {
        SessionType = sessionType;
    }
}
