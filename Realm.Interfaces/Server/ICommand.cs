namespace Realm.Interfaces.Server;

public interface ICommand
{
    string CommandName { get; }

    void HandleCommand(string command);
}
