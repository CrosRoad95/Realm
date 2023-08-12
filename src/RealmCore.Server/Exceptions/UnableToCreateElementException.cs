namespace RealmCore.Server.Exceptions;

public class UnableToCreateElementException : Exception
{
    public string ElementType { get; }

    public UnableToCreateElementException(string message, string elementType) : base(message)
    {
        ElementType = elementType;
    }
}
