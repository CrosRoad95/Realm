namespace RealmCore.Tests.Classes;

internal class TestConsoleCommands : IConsole
{
    public event Action<Entity, string?>? CommandExecuted;

    public void Start()
    {
        throw new NotImplementedException();
    }
}
