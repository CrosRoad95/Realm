namespace Realm.Domain.Exceptions;
public class SessionAlreadyRunningException : Exception
{
    public SessionAlreadyRunningException() : base("Session already started") { }
}
