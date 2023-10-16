
namespace RealmCore.Server.Services;

public interface IBrowserGuiService
{
    string KeyName { get; }

    event Action<Entity>? Ready;

    string GenerateKey();
    void AuthorizeEntity(string key, Entity entity);
    void UnauthorizeEntity(Entity entity);
    bool TryGetEntityByKey(string key, out Entity? entity);
    bool TryGetKeyByEntity(Entity entity, out string? key);
    void RelayEntityLoggedIn(Entity entity);
}
