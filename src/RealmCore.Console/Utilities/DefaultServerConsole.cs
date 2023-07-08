namespace RealmCore.Console.Utilities;

internal class DefaultServerConsole : IConsole
{
    private readonly IECS _ecs;

    public event Action<Entity, string?>? CommandExecuted;

    public DefaultServerConsole(IECS ecs)
    {
        _ecs = ecs;
    }

    public void Start()
    {
        try
        {
            while (true)
            {
                var line = System.Console.ReadLine();
                if (line == null)
                    break;
                CommandExecuted?.Invoke(_ecs.Console, line);
            }
        }
        finally
        {
            ;
        }
    }
}
