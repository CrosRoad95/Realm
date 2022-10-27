namespace Realm.Server.Interfaces;

public interface IRPGServer
{
    string MapName { get; set; }
    string GameType { get; set; }

    event Action<Player>? PlayerJoined;
    void AddEventHandler(string eventName, Func<LuaEvent, Task<object?>> callback);
    void AssociateElement(Element element);
    TService GetRequiredService<TService>() where TService : notnull;
}
