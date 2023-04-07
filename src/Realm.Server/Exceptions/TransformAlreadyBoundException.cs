namespace Realm.Server.Exceptions;

public class TransformAlreadyBoundException : Exception
{
    public TransformAlreadyBoundException() : base("Transform already bound with an entity.")
    {
    }
}
