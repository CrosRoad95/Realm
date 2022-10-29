namespace Realm.WebApp.Services;

public class ConsoleService : IConsoleCommands
{
    public event Action<string?>? CommandExecuted;

    public List<string> Logs { get; } = new List<string>();

    public ConsoleService()
    {
    }

    private void Log(string text)
    {
        Logs.Add($"{DateTime.Now:HH:mm:ss}: {text}");
    }

    public void Submit(string inputCommand)
    {
        Log($"> {inputCommand}");
        CommandExecuted?.Invoke(inputCommand);
    }
}
