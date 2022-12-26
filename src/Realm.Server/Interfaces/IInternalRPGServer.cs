namespace Realm.Server.Interfaces;

public interface IInternalRPGServer
{
    string MapName { get; set; }
    string GameType { get; set; }
    ECS ECS { get; }

    event Action<Entity>? PlayerJoined;

    TService GetRequiredService<TService>() where TService : notnull;
}
