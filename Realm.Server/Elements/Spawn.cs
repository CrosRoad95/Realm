namespace Realm.Server.Elements;

public class Spawn : Element, IDisposable
{
    private bool _disposed = false;
    private readonly AuthorizationPoliciesProvider _authorizationPoliciesProvider;
    private string _id;
    private ILogger _logger;

    private readonly bool _isPersistant = PersistantScope.IsPersistant;
    private readonly List<string> _requiredPolices = new();

    public Spawn(AuthorizationPoliciesProvider authorizationPoliciesProvider, ILogger logger)
    {
        _authorizationPoliciesProvider = authorizationPoliciesProvider;
        Position = position;
        Rotation = rotation;
        Destroyed += e => Dispose();

        _logger = logger
            .ForContext<Spawn>()
            .ForContext(new SpawnEnricher(this));
    }

    [NoScriptAccess]
    public void AssignId(string id)
    {
        _id = id;
    }

    public bool IsPersistant()
    {
        CheckIfDisposed();
        return _isPersistant;
    }

    public bool AddRequiredPolicy(string policy)
    {
        CheckIfDisposed();
        _authorizationPoliciesProvider.ValidatePolicy(policy);

        if (_requiredPolices.Contains(policy))
            return false;
        _requiredPolices.Add(policy);
        return true;
    }

    public bool RemoveRequiredPolicy(string policy)
    {
        CheckIfDisposed();
        _authorizationPoliciesProvider.ValidatePolicy(policy);

        if (!_requiredPolices.Contains(policy))
            return false;
        _requiredPolices.Remove(policy);
        return true;
    }

    public async Task<bool> IsAuthorized(RPGPlayer player)
    {
        CheckIfDisposed();
        if(player.Account == null)
            return false;

        foreach (var policyName in _requiredPolices)
        {
            var result = await player.Account.AuthorizePolicy(policyName);
            if (!result)
                return false;
        }
        return true;
    }

    public string LongUserFriendlyName() => Name;
    public override string ToString() => Name;

    private void CheckIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    public void Dispose()
    {
        _disposed = true;
    }
}
