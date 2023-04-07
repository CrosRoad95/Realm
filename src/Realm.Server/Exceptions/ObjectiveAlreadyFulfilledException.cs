namespace Realm.Server.Exceptions;

public class ObjectiveAlreadyFulfilledException : Exception
{
    public ObjectiveAlreadyFulfilledException()
        : base("Objective has already been fulfilled.")
    {
    }
}