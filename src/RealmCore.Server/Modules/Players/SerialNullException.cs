namespace RealmCore.Server.Modules.Players;

public class SerialNullException : Exception
{
    public SerialNullException() { }
    public SerialNullException(string message) : base(message) { }
}
