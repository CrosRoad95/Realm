namespace RealmCore.Server.Modules.Users;

public class SerialNullException : Exception
{
    public SerialNullException() { }
    public SerialNullException(string message) : base(message) { }
}
