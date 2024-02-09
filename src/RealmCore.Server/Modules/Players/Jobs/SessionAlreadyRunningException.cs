namespace RealmCore.Server.Modules.Players.Jobs;
public class SessionAlreadyRunningException : Exception
{
    public SessionAlreadyRunningException() : base("Session already started") { }
}
