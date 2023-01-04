namespace Realm.Domain.Exceptions;

public class TransformAlreadyBoundException : Exception
{
    public TransformAlreadyBoundException() : base("Transform already bound with an entity.")
    {
    }
}
