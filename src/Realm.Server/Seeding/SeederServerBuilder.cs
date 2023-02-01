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
    private readonly IGroupService _groupService;
    private readonly IEntityFactory _entityFactory;
    private readonly ILogger _logger;

    private readonly Dictionary<string, User> _createdUsers = new();
    public SeederServerBuilder(ILogger logger,
        EntityByStringIdCollection elementByStringIdCollection,
        IServerFilesProvider serverFilesProvider, ECS ecs, UserManager<User> userManager, RoleManager<Role> roleManager,
        IGroupService groupService, IEntityFactory entityFactory)
    {
        _elementByStringIdCollection = elementByStringIdCollection;
        _serverFilesProvider = serverFilesProvider;
        _ecs = ecs;
        _userManager = userManager;
        _roleManager = roleManager;
        _groupService = groupService;
        _entityFactory = entityFactory;
        _logger = logger.ForContext<SeederServerBuilder>();
    }

    private void AssignElementToId(Entity entity, string id)
    {
        var succeed = _elementByStringIdCollection.AssignEntityToId(entity, id);
        if (!succeed)
            throw new Exception($"Failed to assign seeded element to id {id} because it is already in used.");
    }

    private void BuildBlips(Dictionary<string, BlipSeedData> blips)
    {
        foreach (var pair in blips)
        {
            var entity = _entityFactory.CreateBlip((BlipIcon)pair.Value.Icon, pair.Value.Position, pair.Key);
            AssignElementToId(entity, pair.Key);
            _logger.Information("Seeder: Created blip of id {elementId} with icon {blipIcon} at {position}", pair.Key, pair.Value.Icon, pair.Value.Position);
        }
    }

    private void BuildPickups(Dictionary<string, PickupSeedData> pickups)
    {
        foreach (var pair in pickups)
        {
            var entity = _entityFactory.CreatePickup(pair.Value.Model, pair.Value.Position, pair.Key);
            if (pair.Value.Text3d != null)
            {
                entity.AddComponent(new Text3dComponent(pair.Value.Text3d, new Vector3(0, 0, 0.75f)));
            }
            AssignElementToId(entity, pair.Key);
            _logger.Information("Seeder: Created pickup of id {elementId} with icon {pickupModel} at {position}", pair.Key, pair.Value.Model, pair.Value.Position);
        }
    }
    
    private void BuildMarkers(Dictionary<string, MarkerSeedData> markers)
    {
        foreach (var pair in markers)
        {
            var entity = _entityFactory.CreateMarker(pair.Value.MarkerType, pair.Value.Position, 0, 0, pair.Key);
            AssignElementToId(entity, pair.Key);
            _logger.Information("Seeder: Created marker of id {elementId} at {position}", pair.Key, pair.Value.Position);
        }
    }
    
    private async Task BuildGroups(Dictionary<string, GroupSeedData> groups)
    {
        foreach (var pair in groups)
        {
            bool created = false;
            Domain.Concepts.Group group;
            try
            {
                group = await _groupService.CreateGroup(pair.Key, pair.Value.Shortcut, pair.Value.GroupKind);
                created = true;
            }
            catch(Exception) // Group already exists
            {
                group = await _groupService.GetGroupByName(pair.Key) ?? throw new InvalidOperationException();
            }

            foreach (var item in pair.Value.Members)
            {
                try
                {
                    await _groupService.AddMember(group.name, _createdUsers[item.Key].Id, item.Value.Rank, item.Value.RankName);
                }
                catch(Exception) // Maybe member is already in group
                {

                }
            }
            if(created)
                _logger.Information("Seeder: Created group {elementId} with members {members}", pair.Key, pair.Value.Members.Select(x => x.Key));
            else
                _logger.Information("Seeder: Updated group {elementId} with members {members}", pair.Key, pair.Value.Members.Select(x => x.Key));
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
        var seedDatas = _serverFilesProvider.GetFiles(_basePath).Select(seedFileName => JsonConvert.DeserializeObject<SeedData>(File.ReadAllText(seedFileName)));
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
        await BuildGroups(seedData.Groups);
        _createdUsers.Clear();
    }
}