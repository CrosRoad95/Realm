namespace RealmCore.Server.Exceptions;

public class CommandExistsException : Exception
{
    public CommandExistsException(string message) : base(message) { }
}
