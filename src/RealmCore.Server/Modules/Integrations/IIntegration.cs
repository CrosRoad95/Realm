namespace RealmCore.Server.Modules.Integrations;

public interface IIntegration
{
    RealmPlayer Player { get; }

    event Action<IIntegration>? Created;
    event Action<IIntegration>? Removed;

    bool IsIntegrated();
    bool TryRemove();
}
