using Realm.Module.Scripting.Interfaces;

namespace Realm.Server.Interfaces;

public interface IRPGServer
{
    string MapName { get; set; }
    string GameType { get; set; }

    event Action<Player>? PlayerJoined;
    event Action? ServerReloaded;

    void AssociateElement(Element element);
    TService GetRequiredService<TService>() where TService : notnull;
    void InitializeScripting(IScriptingModuleInterface scriptingModuleInterface);
}
