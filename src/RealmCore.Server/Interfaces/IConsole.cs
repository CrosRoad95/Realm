namespace RealmCore.Server.Interfaces;

public interface IConsole
{
    event Action<Entity, string?>? CommandExecuted;

    void Start();
}
