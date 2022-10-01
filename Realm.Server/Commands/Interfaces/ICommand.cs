namespace Realm.Server.Commands.Interfaces;

internal interface ICommand
{
    string CommandName { get; }

    void HandleCommand(string command);
}
