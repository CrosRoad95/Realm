namespace RealmCore.Interfaces.Server;

public interface IConsole
{
    event Action<string?>? CommandExecuted;

    void Start();
}
