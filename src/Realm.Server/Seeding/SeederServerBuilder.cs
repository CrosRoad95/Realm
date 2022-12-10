using FluentValidation;
using Realm.Server.Serialization.Yaml;
using System.Runtime.CompilerServices;
using YamlDotNet.Serialization.NamingConventions;
using static Realm.Server.Seeding.SeedData;
using VehicleUpgrade = Realm.Domain.Upgrades.VehicleUpgrade;

namespace Realm.Server.Seeding;

internal sealed class SeederServerBuilder
{
    private const string _basePath = "Seed";
    private readonly IRPGElementsFactory _elementFunctions;
    private readonly IdentityScriptingFunctions _identityFunctions;
    private readonly ElementByStringIdCollection _elementByStringIdCollection;
    private readonly VehicleUpgradeByStringCollection _vehicleUpgradeByStringCollection;
    private readonly IServerFilesProvider _serverFilesProvider;
    private readonly ILogger _logger;
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .WithTypeConverter(new Vector3Converter())
        .Build();

    private readonly Dictionary<string, PlayerAccount> _createdAccounts = new();
    public SeederServerBuilder(IRPGElementsFactory elementFunctions, IdentityScriptingFunctions identityFunctions, ILogger logger,
        ElementByStringIdCollection elementByStringIdCollection, VehicleUpgradeByStringCollection vehicleUpgradeByStringCollection,
        IServerFilesProvider serverFilesProvider)
    {
        _elementFunctions = elementFunctions;
        _identityFunctions = identityFunctions;
        _elementByStringIdCollection = elementByStringIdCollection;
        _vehicleUpgradeByStringCollection = vehicleUpgradeByStringCollection;
        _serverFilesProvider = serverFilesProvider;
        _logger = logger.ForContext<SeederServerBuilder>();
    }

    private void AssignElementToId(Element element, string id)
    {
        var succeed = _elementByStringIdCollection.AssignElementToId(element, id);
        if (!succeed)
            throw new Exception($"Failed to assign seeded element to id {id} because it is already in used.");
    }

    private void BuildSpawns(Dictionary<string, Spawn> spawns)
    {
        foreach (var pair in spawns)
        {
            var spawn = _elementFunctions.CreateSpawn(pair.Value.Position, pair.Value.Rotation, pair.Value.Name);
            AssignElementToId(spawn, pair.Key);
            _logger.Information("Seeder: Created spawn of id {elementId} at {position}", pair.Key, pair.Value.Position);
        }
    }

    private void BuildBlips(Dictionary<string, SeedData.Blip> blips)
    {
        foreach (var pair in blips)
        {
            var blip = _elementFunctions.CreateBlip(pair.Value.Position, pair.Value.Icon);
            AssignElementToId(blip, pair.Key);
            _logger.Information("Seeder: Created blip of id {elementId} with icon {blipIcon} at {position}", pair.Key, pair.Value.Icon, pair.Value.Position);
        }
    }

    private void BuildPickups(Dictionary<string, SeedData.Pickup> pickups)
    {
        foreach (var pair in pickups)
        {
            var pickup = _elementFunctions.CreatePickup(pair.Value.Position, pair.Value.Model);
            AssignElementToId(pickup, pair.Key);
            _logger.Information("Seeder: Created pickup of id {elementId} with icon {pickupModel} at {position}", pair.Key, pair.Value.Model, pair.Value.Position);
        }
    }

    private async Task BuildFractions(Dictionary<string, Fraction> fractionsData)
    {
        foreach (var pair in fractionsData)
        {
            var fraction = _elementFunctions.CreateFraction(pair.Value.Code, pair.Value.Name, pair.Value.Position);
            if (pair.Value.Members != null)
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
                _logger.Information("Seeder: Created fraction of id {elementId} with name '{fractionName}' with no members", pair.Key, pair.Value.Name);
            AssignElementToId(fraction, pair.Key);
        }
    }

    private async Task BuildIdentityRoles(List<string> roles)
    {
        var existingRoles = await _identityFunctions.GetAllRoles();
        foreach (var roleName in roles)
        {
            if (!existingRoles.Any(x => x.Name == roleName))
                await _identityFunctions.CreateRole(roleName);
        }
    }

    private async Task BuildIdentityAccounts(Dictionary<string, Account> accounts)
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


    private void BuildUpgrades(Dictionary<string, VehicleUpgradeDescription> upgradePairs)
    {
        foreach (var upgradePair in upgradePairs)
        {
            var upgrade = new VehicleUpgrade
            {
                MaxVelocity = new VehicleUpgrade.UpgradeDescription(upgradePair.Value.MaxVelocity),
                EngineAcceleration = new VehicleUpgrade.UpgradeDescription(upgradePair.Value.EngineAcceleration),
            };
            if (!_vehicleUpgradeByStringCollection.AssignElementToId(upgrade, upgradePair.Key))
            {
                _logger.Warning("Found duplicated upgrade: {upgradeName}", upgradePair.Key);
            }
        }
    }

    public async Task Build()
    {
        var result = new JObject();
        var seedDatas = _serverFilesProvider.GetFiles(_basePath).Select(seedFileName => _deserializer.Deserialize<SeedData>(File.ReadAllText(seedFileName)));
        foreach (var sourceObject in seedDatas)
        {
            var @object = JObject.Parse(JsonConvert.SerializeObject(sourceObject));
            result.Merge(@object, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            });
        }

        var seedData = result.ToObject<SeedData>();
        if (seedData == null)
            throw new Exception("Failed to load seed data.");

        var seedValidator = new SeedValidator();
        await seedValidator.ValidateAndThrowAsync(seedData);
        await BuildFrom(seedData);
    }
    
    private async Task BuildFrom(SeedData seedData)
    {
        using var _ = new PersistantScope();
        BuildUpgrades(seedData.Upgrades);
        await BuildIdentityRoles(seedData.Roles);
        await BuildIdentityAccounts(seedData.Accounts);
        BuildSpawns(seedData.Spawns);
        BuildBlips(seedData.Blips);
        BuildPickups(seedData.Pickups);
        await BuildFractions(seedData.Fractions);
    }
}