using Serilog.Events;

namespace Realm.WebApp.Services;

public class ConsoleService : IConsoleCommands
{
    public event Action<string?>? CommandExecuted;
    public event Action? LogAdded;

    public List<string> Logs { get; } = new List<string>();

    public ConsoleService(SubscribableLogsSink consoleLogsSink)
    {
        consoleLogsSink.LogEmited += ConsoleLogsSink_LogEmited;
    }

    private void ConsoleLogsSink_LogEmited(LogEvent logEvent)
    {
        Log(logEvent.RenderMessage());
    }

    private void Log(string? text)
    {
        if (text == null)
            return;

        Logs.Add($"{DateTime.Now:HH:mm:ss}: {text}");
        LogAdded?.Invoke();
    }

    public void Submit(string inputCommand)
    {
        Log($"> {inputCommand}");
        CommandExecuted?.Invoke(inputCommand);
    }
}
