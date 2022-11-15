using Serilog;

namespace Realm.MTARPGServer;

internal class SeederServerBuilder
{
    private readonly ElementScriptingFunctions _elementFunctions;
    private readonly IdentityScriptingFunctions _identityFunctions;
    private readonly ILogger _logger;
    private readonly Dictionary<string, PlayerAccount> _createdAccounts = new();
    public SeederServerBuilder(ElementScriptingFunctions elementFunctions, IdentityScriptingFunctions identityFunctions, ILogger logger)
    {
        _elementFunctions = elementFunctions;
        _identityFunctions = identityFunctions;
        _logger = logger.ForContext<SeederServerBuilder>();
    }

    private void BuildSpawns(Dictionary<string, SeedData.Spawn> spawns)
    {
        foreach (var pair in spawns)
        {
            _elementFunctions.CreateSpawn(pair.Key, pair.Value.Name, pair.Value.Position, pair.Value.Rotation);
            _logger.Information("Seeder: Created spawn {spawnName} at {position}", pair.Value.Name, pair.Value.Position);
        }
    }
    
    private void BuildBlips(Dictionary<string, SeedData.Blip> blips)
    {
        foreach (var pair in blips)
        {
            _elementFunctions.CreateBlip(pair.Value.Icon, pair.Value.Position);
            _logger.Information("Seeder: Created blip {blipIcon} at {position}", pair.Value.Icon, pair.Value.Position);
        }
    }
    
    private void BuildPickups(Dictionary<string, SeedData.Pickup> pickups)
    {
        foreach (var pair in pickups)
        {
            _elementFunctions.CreatePickup(pair.Value.Position, pair.Value.Model);
            _logger.Information("Seeder: Created pickup {pickupModel} at {position}", pair.Value.Model, pair.Value.Position);
        }
    }
    
    private async Task BuildFractions(Dictionary<string, SeedData.Fraction> fractionsData)
    {
        foreach (var pair in fractionsData)
        {
            var fraction = _elementFunctions.CreateFraction(pair.Key, pair.Value.Name, pair.Value.Position);
            if(pair.Value.Members != null)
            {
                _logger.Information("Seeder: Created fraction {fractionCode} with members:", pair.Key);
                foreach (var memberPair in pair.Value.Members)
                {
                    if (!_createdAccounts.ContainsKey(memberPair.Key))
                        throw new KeyNotFoundException($"Failed to find account name `{memberPair.Key}`");
                    var account = _createdAccounts[memberPair.Key];
                    await fraction.InternalAddMember(account, memberPair.Value.Permissions);
                    _logger.Information("Seeder: Added member {accountName} to fraction {fractionCode} with permissions {permissions}", memberPair.Key, pair.Key, memberPair.Value.Permissions);
                }
            }
            else
                _logger.Information("Seeder: Created fraction {fractionCode} with no members", pair.Key);
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

    private async Task BuildIdentityAccounts(Dictionary<string, SeedData.Account> accounts)
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
            _createdAccounts.Add(pair.Key, account);
        }
    }

    public async Task BuildFrom(SeedData seed)
    {
        using var _ = new PersistantScope();
        await BuildIdentityAccounts(seed.Accounts);
        BuildSpawns(seed.Spawns);
        BuildBlips(seed.Blips);
        BuildPickups(seed.Pickups);
        await BuildFractions(seed.Fractions);
        await BuildIdentityRoles(seed.Roles);
    }
}