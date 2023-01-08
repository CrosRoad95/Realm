namespace Realm.Domain.Exceptions;

public class ObjectiveAlreadyFulfilledException : Exception
{
    public ObjectiveAlreadyFulfilledException()
        : base("Objective has already been fulfilled.")
    {
    }
}