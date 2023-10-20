using System.Security.Cryptography;

namespace RealmCore.Server.Services;

internal class BrowserGuiService : IBrowserGuiService
{
    private readonly ConcurrentDictionary<string, Entity> _browserEntities = new();

    private readonly RandomNumberGenerator _randomNumberGenerator;
    private readonly object _lock = new();
    private readonly byte[] _bytes = new byte[64];
    public event Action<Entity>? Ready;
    public string KeyName => "guiKey";

    public BrowserGuiService()
    {
        _randomNumberGenerator = RandomNumberGenerator.Create();
    }

    public void RelayEntityLoggedIn(Entity entity)
    {
        Ready?.Invoke(entity);
    }

    public string GenerateKey()
    {
        lock (_lock)
        {
            _randomNumberGenerator.GetBytes(_bytes);

            return Convert.ToBase64String(_bytes).Replace('+', '-').Replace('/', '_');
        }
    }
    public void AuthorizeEntity(string key, Entity entity)
    {
        _browserEntities.TryAdd(key, entity);
    }

    public void UnauthorizeEntity(Entity entity)
    {
        var itemsToRemove = _browserEntities.Where(x => x.Value == entity).FirstOrDefault();

        _browserEntities.TryRemove(itemsToRemove.Key, out var _);
    }
    
    public bool TryGetKeyByEntity(Entity entity, out string? key)
    {
        var browserEntities = _browserEntities.Where(x => x.Value == entity).ToList();
        if (browserEntities.Count == 0)
        {
            key = null;
            return false;
        }
        key = browserEntities[0].Key;
        return true;
    }

    public bool TryGetEntityByKey(string key, out Entity? entity)
    {
        bool found = _browserEntities.TryGetValue(key, out entity);

        return found;
    }
}
