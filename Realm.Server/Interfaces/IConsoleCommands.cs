namespace Realm.Server.Interfaces;

public interface IConsoleCommands
{
    event Action<string?>? CommandExecuted;
}
