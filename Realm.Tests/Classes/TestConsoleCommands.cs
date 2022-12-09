using Realm.Interfaces.Server;

namespace Realm.Tests.Classes;

internal class TestConsoleCommands : IConsole
{
    public event Action<string?>? CommandExecuted;
}
