using Realm.Scripting.Scopes;

namespace Realm.MTARPGServer;

internal class ProvisioningServerBuilder
{
    private readonly ElementFunctions _elementFunctions;
    private readonly IdentityFunctions _identityFunctions;

    public ProvisioningServerBuilder(ElementFunctions elementFunctions, IdentityFunctions identityFunctions)
    {
        _elementFunctions = elementFunctions;
        _identityFunctions = identityFunctions;
    }

    private void BuildSpawns(Dictionary<string, Provisioning.Spawn> spawns)
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

    private async Task BuildIdentityAccounts(Dictionary<string, Provisioning.Account> accounts)
    {
        foreach (var pair in accounts)
        {
            var account = await _identityFunctions.FindAccountByUserName(pair.Key);
            if (account == null)
                account = await _identityFunctions.CreateAccount(pair.Key, pair.Value.Password);

            await account.RemoveAllClaims();
            await account.AddClaims(pair.Value.Claims);
            await account.AddRoles(pair.Value.Roles);
            await account.AddClaim("provisioned", "true");
        }
    }

    public async Task BuildFrom(Provisioning provisioning)
    {
        using var _ = new PersistantScope();
        BuildSpawns(provisioning.Spawns);
        await BuildIdentityRoles(provisioning.Roles);
        await BuildIdentityAccounts(provisioning.Accounts);
    }
}