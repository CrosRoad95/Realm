﻿using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Realm.Persistance.Data;
using Realm.Server.Serialization.Yaml;
using System.Security.Claims;
using YamlDotNet.Serialization.NamingConventions;
using static Realm.Server.Seeding.SeedData;
using VehicleUpgrade = Realm.Domain.Upgrades.VehicleUpgrade;

namespace Realm.Server.Seeding;

internal sealed class SeederServerBuilder
{
    private const string _basePath = "Seed";
    private readonly EntityByStringIdCollection _elementByStringIdCollection;
    private readonly VehicleUpgradeByStringCollection _vehicleUpgradeByStringCollection;
    private readonly IServerFilesProvider _serverFilesProvider;
    private readonly IInternalRPGServer _rpgServer;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ILogger _logger;
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .WithTypeConverter(new Vector3Converter())
        .Build();

    private readonly Dictionary<string, User> _createdUsers = new();
    public SeederServerBuilder(ILogger logger,
        EntityByStringIdCollection elementByStringIdCollection, VehicleUpgradeByStringCollection vehicleUpgradeByStringCollection,
        IServerFilesProvider serverFilesProvider, IInternalRPGServer rpgServer, UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        _elementByStringIdCollection = elementByStringIdCollection;
        _vehicleUpgradeByStringCollection = vehicleUpgradeByStringCollection;
        _serverFilesProvider = serverFilesProvider;
        _rpgServer = rpgServer;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger.ForContext<SeederServerBuilder>();
    }

    private void AssignElementToId(Entity entity, string id)
    {
        var succeed = _elementByStringIdCollection.AssignEntityToId(entity, id);
        if (!succeed)
            throw new Exception($"Failed to assign seeded element to id {id} because it is already in used.");
    }

    private Entity CreateEntity(string key)
    {
        var entity = _rpgServer.CreateEntity(key);
        AssignElementToId(entity, key);
        return entity;
    }

    private void BuildBlips(Dictionary<string, BlipSeedData> blips)
    {
        foreach (var pair in blips)
        {
            var blipEntity = CreateEntity(pair.Key);
            blipEntity.AddComponent(new BlipElementComponent(new Blip(Vector3.Zero, (BlipIcon)pair.Value.Icon, 250)));
            blipEntity.Transform.Position = pair.Value.Position;
            _logger.Information("Seeder: Created blip of id {elementId} with icon {blipIcon} at {position}", pair.Key, pair.Value.Icon, pair.Value.Position);
        }
    }

    private void BuildPickups(Dictionary<string, PickupSeedData> pickups)
    {
        foreach (var pair in pickups)
        {
            var blipEntity = CreateEntity(pair.Key);
            blipEntity.AddComponent(new PickupElementComponent(new Pickup(Vector3.Zero, pair.Value.Model)));
            blipEntity.Transform.Position = pair.Value.Position;
            _logger.Information("Seeder: Created pickup of id {elementId} with icon {pickupModel} at {position}", pair.Key, pair.Value.Model, pair.Value.Position);
        }
    }

    private async Task BuildIdentityRoles(List<string> roles)
    {
        var existingRoles = await _roleManager.Roles.ToListAsync();
        foreach (var roleName in roles)
        {
            if (!existingRoles.Any(x => x.Name == roleName))
                await _roleManager.CreateAsync(new Role
                {
                    Name = roleName
                });
        }
    }

    private async Task BuildIdentityAccounts(Dictionary<string, AccountSeedData> accounts)
    {
        foreach (var pair in accounts)
        {
            var user = await _userManager.FindByNameAsync(pair.Key);
            if (user == null)
            {
                var identityResult = await _userManager.CreateAsync(new User
                {
                    UserName = pair.Key,
                }, pair.Value.Password);
                if(identityResult.Succeeded)
                {
                    user = await _userManager.FindByNameAsync(pair.Key);
                }
                else
                {
                    throw new Exception();
                }
            }

            var claims = pair.Value.Claims.Select(x => new Claim(x.Key, x.Value))
                .Concat(new List<Claim>
                {
                    new("seeded", "true"),
                    new("persistant", "true"),
                });

            await _userManager.RemoveClaimsAsync(user, await _userManager.GetClaimsAsync(user));
            await _userManager.AddClaimsAsync(user, claims);
            await _userManager.AddToRolesAsync(user, pair.Value.Roles);

            _createdUsers.Add(pair.Key, user);
        }
    }


    private void BuildUpgrades(Dictionary<string, VehicleUpgradeDescriptionSeedData> upgradePairs)
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
        BuildUpgrades(seedData.Upgrades);
        await BuildIdentityRoles(seedData.Roles);
        await BuildIdentityAccounts(seedData.Accounts);
        BuildBlips(seedData.Blips);
        BuildPickups(seedData.Pickups);
        _createdUsers.Clear();
    }
}