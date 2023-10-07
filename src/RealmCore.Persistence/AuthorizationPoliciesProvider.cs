namespace RealmCore.Persistence;

public class AuthorizationPoliciesProvider
{
    private readonly HashSet<string> _policies;
    public IEnumerable<string> Policies => _policies;

    public AuthorizationPoliciesProvider(IEnumerable<string> policies)
    {
        _policies = new HashSet<string>(policies);
    }

    public void ValidatePolicy(string policy)
    {
        if (!_policies.Contains(policy))
            throw new NotSupportedException($"Not supported policy '{policy}'");
    }
}
