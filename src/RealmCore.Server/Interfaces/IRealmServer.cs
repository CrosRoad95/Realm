namespace RealmCore.Server.Interfaces;

public interface IRealmServer
{
    event Action? ServerStarted;

    T GetRequiredService<T>() where T : notnull;
    void Start();
}
