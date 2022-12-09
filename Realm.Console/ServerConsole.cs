using Realm.Interfaces.Server;

namespace Realm.Console;

internal class ServerConsole : IConsole
{
    public event Action<string?>? CommandExecuted;

    public void Start()
    {
        try
        {
            while (true)
            {
                var line = System.Console.ReadLine();
                CommandExecuted?.Invoke(line);
            }
        }
        finally
        {

        }
    }
}
