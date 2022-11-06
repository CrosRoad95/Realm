using Realm.Scripting.Scopes;

namespace Realm.MTARPGServer;

internal class SeederServerBuilder
{
    private readonly ElementFunctions _elementFunctions;
    private readonly IdentityFunctions _identityFunctions;

    public SeederServerBuilder(ElementFunctions elementFunctions, IdentityFunctions identityFunctions)
    {
        _elementFunctions = elementFunctions;
        _identityFunctions = identityFunctions;
    }

    private void BuildSpawns(Dictionary<string, Seed.Spawn> spawns)
    {
        foreach (var pair in spawns)
        {
            _elementFunctions.CreateSpawn(pair.Key, pair.Value.Name, pair.Value.Position, pair.Value.Rotation);
        }
    }

    private async Task BuildIdentityRoles(List<string> roles)
    {
        var existingRoles = await _identityFunctions.GetAllRoles();
        foreach (var roleName in roles)
        {
            if(!existingRoles.Any(x => x.Name == roleName))
                await _identityFunctions.CreateRole(roleName);
        }
    }

    private async Task BuildIdentityAccounts(Dictionary<string, Seed.Account> accounts)
    {
        foreach (var pair in accounts)
        {
            var account = await _identityFunctions.FindAccountByUserName(pair.Key);
            if (account == null)
                account = await _identityFunctions.CreateAccount(pair.Key, pair.Value.Password);

            await account.RemoveAllClaims();
            await account.AddClaims(pair.Value.Claims);
            await account.AddRoles(pair.Value.Roles);
            await account.AddClaim("seeded", "true");
            await account.AddClaim("persistant", "true");
        }
    }

    public async Task BuildFrom(Seed seed)
    {
        using var _ = new PersistantScope();
        BuildSpawns(seed.Spawns);
        await BuildIdentityRoles(seed.Roles);
        await BuildIdentityAccounts(seed.Accounts);
    }
}