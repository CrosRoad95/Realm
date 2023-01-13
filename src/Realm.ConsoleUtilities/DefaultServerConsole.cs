using Realm.Interfaces.Server;

namespace Realm.ConsoleUtilities;

internal class DefaultServerConsole : IConsole
{
    public event Action<string?>? CommandExecuted;

    public void Start()
    {
        try
        {
            while (true)
            {
                var line = Console.ReadLine();
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
