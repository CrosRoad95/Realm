namespace RealmCore.Tests.Classes;

internal class TestConsoleCommands : IConsole
{
    public event Action<string?>? CommandExecuted;

    public void Start()
    {
        throw new NotImplementedException();
    }
}
