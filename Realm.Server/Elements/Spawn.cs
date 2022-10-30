namespace Realm.Server.Elements;

public class Spawn : Element, IDisposable
{
    private bool _disposed = false;
    private readonly AuthorizationPoliciesProvider _authorizationPoliciesProvider;
    private readonly string _id;

    private readonly bool _isPersistant = PersistantScope.IsPersistant;
    private readonly List<string> _requiredPolices = new();
    public Spawn(AuthorizationPoliciesProvider authorizationPoliciesProvider, string id, string name, Vector3 position, Vector3 rotation)
    {
        Name = name;
        _authorizationPoliciesProvider = authorizationPoliciesProvider;
        _id = id;
        Position = position;
        Rotation = rotation;
        Destroyed += e => Dispose();
    }

    private void CheckIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
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

    public async Task<bool> Authorize(RPGPlayer player)
    {
        CheckIfDisposed();
        foreach (var policyName in _requiredPolices)
        {
            var result = await player.AuthorizeInternal(policyName);
            if (result == null)
                return false;

            if (!result.Succeeded)
                throw new UnauthorizedAccessException($"Policy '{policyName}' failed, reason: {result}");
        }
        return true;
    }

    public override string ToString() => "Spawn";

    public void Dispose()
    {
        _disposed = true;
    }
}
