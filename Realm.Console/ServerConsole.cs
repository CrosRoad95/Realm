namespace Realm.Console;

internal class ServerConsole : IConsoleCommands
{
    public event Action<string?>? CommandExecuted;

    public void Start()
    {
        while (true)
        {
            var line = System.Console.ReadLine();
            CommandExecuted?.Invoke(line);
        }
    }   
}
