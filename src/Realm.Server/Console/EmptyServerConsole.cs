namespace Realm.Server.Console;

public class EmptyServerConsole : IConsole
{
    private EmptyServerConsole()
    {

    }

    public static IConsole Instance = new EmptyServerConsole();

    public event Action<string?>? CommandExecuted;

    public void Start()
    {
    }
}
