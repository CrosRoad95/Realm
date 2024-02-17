namespace RealmCore.Server.Modules.Players.Jobs;

public class SessionNotFoundException : Exception
{
    public Type SessionType { get; }

    public SessionNotFoundException(Type sessionType)
    {
        SessionType = sessionType;
    }
}
