namespace Realm.Server.Interfaces;

public interface IInternalRPGServer
{
    string MapName { get; set; }
    string GameType { get; set; }

    event Action<Player>? PlayerJoined;
    event Action? ServerReloaded;

    Entity CreateEntity(string name);
    Task DoReload();
    TService GetRequiredService<TService>() where TService : notnull;
    void InitializeScripting(IScriptingModuleInterface scriptingModuleInterface);
}
