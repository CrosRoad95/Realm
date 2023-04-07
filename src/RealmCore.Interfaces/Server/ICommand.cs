namespace RealmCore.Interfaces.Server;

public interface ICommand
{
    Task HandleCommand(string command);
}
