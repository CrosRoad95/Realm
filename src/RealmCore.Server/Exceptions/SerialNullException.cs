namespace RealmCore.Server.Exceptions;

public class SerialNullException : Exception
{
    public SerialNullException() { }
    public SerialNullException(string message) : base(message) { }
}
