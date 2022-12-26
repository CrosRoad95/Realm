namespace Realm.Interfaces.Server;

public interface ICommand
{
    string CommandName { get; }

    Task HandleCommand(string command);
}
