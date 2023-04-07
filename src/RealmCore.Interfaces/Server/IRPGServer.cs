namespace RealmCore.Interfaces.Server;

public interface IRPGServer
{
    event Action? ServerStarted;

    TService GetRequiredService<TService>() where TService : notnull;
    Task Start();
    Task Stop();
}
