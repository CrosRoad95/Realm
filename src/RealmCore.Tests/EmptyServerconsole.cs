namespace RealmCore.Tests;

public class EmptyServerConsole : IConsole
{
    public EmptyServerConsole()
    {

    }

    public static IConsole Instance = new EmptyServerConsole();

    public event Action<Entity, string?>? CommandExecuted;

    public void Start()
    {
    }
}
