namespace RealmCore.Server.Exceptions;
public class SessionAlreadyRunningException : Exception
{
    public SessionAlreadyRunningException() : base("Session already started") { }
}
