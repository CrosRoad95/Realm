namespace Realm.Interfaces.Server;

public interface IConsoleCommands
{
    event Action<string?>? CommandExecuted;
}
