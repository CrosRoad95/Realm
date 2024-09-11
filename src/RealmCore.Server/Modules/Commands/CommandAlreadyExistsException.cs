namespace RealmCore.Server.Modules.Commands;

public class CommandAlreadyExistsException : Exception
{
    public CommandAlreadyExistsException(string message) : base(message) { }
}
