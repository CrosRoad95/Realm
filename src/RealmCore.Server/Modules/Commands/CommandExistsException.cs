namespace RealmCore.Server.Modules.Commands;

public class CommandExistsException : Exception
{
    public CommandExistsException(string message) : base(message) { }
}
