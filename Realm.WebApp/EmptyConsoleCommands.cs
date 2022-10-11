using Realm.Interfaces.Server;

namespace Realm.WebApp;

public class EmptyConsoleCommands : IConsoleCommands
{
    public event Action<string?>? CommandExecuted;
}
