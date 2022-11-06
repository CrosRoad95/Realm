using Realm.Interfaces.Server;

namespace Realm.Tests.Classes;

internal class TestConsoleCommands : IConsoleCommands
{
    public event Action<string?>? CommandExecuted;
}
