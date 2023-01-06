using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Realm.Persistance.Data;
using Realm.Server.Serialization.Yaml;
using System.Security.Claims;
using YamlDotNet.Serialization.NamingConventions;
using static Realm.Server.Seeding.SeedData;

namespace Realm.Server.Seeding;

internal sealed class SeederServerBuilder
{
    private const string _basePath = "Seed";
    private readonly EntityByStringIdCollection _elementByStringIdCollection;
    private readonly IServerFilesProvider _serverFilesProvider;
    private readonly ECS _ecs;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ILogger _logger;
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .WithTypeConverter(new Vector3Converter())
        .Build();

    private readonly Dictionary<string, User> _createdUsers = new();
    public SeederServerBuilder(ILogger logger,
        EntityByStringIdCollection elementByStringIdCollection,
        IServerFilesProvider serverFilesProvider, ECS ecs, UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        _elementByStringIdCollection = elementByStringIdCollection;
        _serverFilesProvider = serverFilesProvider;
        _ecs = ecs;
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

    private Entity CreateEntity(string key, string tag, Action<Entity> entityBuilder)
    {
        var entity = _ecs.CreateEntity(key, tag, entityBuilder);
        AssignElementToId(entity, key);
        return entity;
    }

    private void BuildBlips(Dictionary<string, BlipSeedData> blips)
    {
        foreach (var pair in blips)
        {
            var blipEntity = CreateEntity(pair.Key, Entity.BlipTag, entity =>
            {
                entity.AddComponent(new BlipElementComponent(new Blip(Vector3.Zero, (BlipIcon)pair.Value.Icon, 250)));
                entity.Transform.Position = pair.Value.Position;
            });
            _logger.Information("Seeder: Created blip of id {elementId} with icon {blipIcon} at {position}", pair.Key, pair.Value.Icon, pair.Value.Position);
        }
    }

    private void BuildPickups(Dictionary<string, PickupSeedData> pickups)
    {
        foreach (var pair in pickups)
        {
            var blipEntity = CreateEntity(pair.Key, Entity.PickupTag, entity =>
            {
                entity.AddComponent(new PickupElementComponent(new Pickup(Vector3.Zero, pair.Value.Model)));
                entity.Transform.Position = pair.Value.Position;
            });
            _logger.Information("Seeder: Created pickup of id {elementId} with icon {pickupModel} at {position}", pair.Key, pair.Value.Model, pair.Value.Position);
        }
    }
    
    private void BuildMarkers(Dictionary<string, MarkerSeedData> markers)
    {
        foreach (var pair in markers)
        {
            var blipEntity = CreateEntity(pair.Key, Entity.MarkerTag, entity =>
            {
                var marker = new Marker(Vector3.Zero, pair.Value.MarkerType);
                marker.Color = pair.Value.Color;
                marker.Size = pair.Value.Size;
                entity.AddComponent(new MarkerElementComponent(marker));
                entity.Transform.Position = pair.Value.Position;
            });
            _logger.Information("Seeder: Created marker of id {elementId} at {position}", pair.Key, pair.Value.Position);
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
            }

            if(user == null)
                throw new Exception($"Failed to create user account '{pair.Key}'");

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
        await BuildIdentityRoles(seedData.Roles);
        await BuildIdentityAccounts(seedData.Accounts);
        BuildBlips(seedData.Blips);
        BuildPickups(seedData.Pickups);
        BuildMarkers(seedData.Markers);
        _createdUsers.Clear();
    }
}