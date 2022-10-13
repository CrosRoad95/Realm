namespace Realm.Interfaces.Server;

[Name("Player")]
public interface IRPGPlayer : IElement, IMovable
{
    string Name { get; set; }
    CancellationToken CancellationToken { get; }

    event Action<IRPGPlayer, int>? ResourceReady;

    void Spawn(ISpawn spawn);
    void TriggerClientEvent(string @event, object value);
}
