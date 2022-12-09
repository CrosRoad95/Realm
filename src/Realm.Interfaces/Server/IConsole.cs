namespace Realm.Interfaces.Server;

public interface IConsole
{
    event Action<string?>? CommandExecuted;
}
