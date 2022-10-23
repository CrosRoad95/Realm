namespace Realm.Server.Elements;

public class Spawn : Element
{
    private readonly AuthorizationPoliciesProvider _authorizationPoliciesProvider;
    private readonly string _id;

    private readonly bool _isPersistant = PersistantScope.IsPersistant;
    private readonly List<string> _requiredPolices = new List<string>();
    public Spawn(AuthorizationPoliciesProvider authorizationPoliciesProvider,string id, string name, Vector3 position, Vector3 rotation)
    {
        Name = name;
        _authorizationPoliciesProvider = authorizationPoliciesProvider;
        _id = id;
        Position = position;
        Rotation = rotation;
    }

    public bool IsPersistant() => _isPersistant;

    public bool AddRequiredPolicy(string policy)
    {
        _authorizationPoliciesProvider.ValidatePolicy(policy);

        if (_requiredPolices.Contains(policy))
            return false;
        _requiredPolices.Add(policy);
        return true;
    }
    
    public bool RemoveRequiredPolicy(string policy)
    {
        _authorizationPoliciesProvider.ValidatePolicy(policy);

        if (!_requiredPolices.Contains(policy))
            return false;
        _requiredPolices.Remove(policy);
        return true;
    }

    public async Task<bool> Authorize(RPGPlayer player)
    {
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
}
