namespace Realm.Domain.Exceptions;

public class EntityAccessDefinedException : Exception
{
    public EntityAccessDefinedException() : base() { }
    public EntityAccessDefinedException(string message) : base(message) { }
    public EntityAccessDefinedException(string message, Exception innerException) : base(message, innerException) { }
}
