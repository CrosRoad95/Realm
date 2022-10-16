namespace Realm.WebApp;

public class EmptyConsoleCommands : IConsoleCommands
{
    public event Action<string?>? CommandExecuted;
}
