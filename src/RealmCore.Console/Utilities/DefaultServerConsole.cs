namespace RealmCore.Console.Utilities;

internal class DefaultServerConsole : IConsole
{
    public event Action<string?>? CommandExecuted;

    public void Start()
    {
        try
        {
            while (true)
            {
                var line = System.Console.ReadLine();
                if (line == null)
                    break;
                CommandExecuted?.Invoke(line);
            }
        }
        finally
        {
            ;
        }
    }
}
