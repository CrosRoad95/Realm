using System.Security.Cryptography;

namespace RealmCore.Server.Services;

internal class BrowserGuiService : IBrowserGuiService
{
    private readonly ConcurrentDictionary<string, Entity> _browserEntities = new();
    private readonly ILogger<BrowserGuiService> _logger;

    private readonly RandomNumberGenerator _randomNumberGenerator;
    private readonly object _lock = new();
    private readonly byte[] bytes = new byte[64];

    public string KeyName => "guiKey";

    public BrowserGuiService(ILogger<BrowserGuiService> logger)
    {
        _logger = logger;
        _randomNumberGenerator = RandomNumberGenerator.Create();
    }

    public string GenerateKey()
    {
        lock (_lock)
        {
            _randomNumberGenerator.GetBytes(bytes);

            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_');
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
