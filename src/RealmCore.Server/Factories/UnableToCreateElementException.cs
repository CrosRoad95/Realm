namespace RealmCore.Server.Factories;

public class UnableToCreateElementException : Exception
{
    private readonly string _elementType;
    public string ElementType => _elementType;

    public UnableToCreateElementException(string message, string elementType) : base(message)
    {
        _elementType = elementType;
    }
}
