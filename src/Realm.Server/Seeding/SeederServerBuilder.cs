using static Realm.Server.Seeding.SeedData;

namespace Realm.Server.Seeding;

internal sealed class SeederServerBuilder
{
    private const string _basePath = "Seed";
    private readonly EntityByStringIdCollection _elementByStringIdCollection;
    private readonly IServerFilesProvider _serverFilesProvider;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IGroupService _groupService;
    private readonly IEntityFactory _entityFactory;
    private readonly IFractionService _fractionService;
    private readonly IDb _db;
    private readonly ILogger<SeederServerBuilder> _logger;

    private readonly Dictionary<string, User> _createdAccounts = new();
    public SeederServerBuilder(ILogger<SeederServerBuilder> logger,
        EntityByStringIdCollection elementByStringIdCollection,
        IServerFilesProvider serverFilesProvider, UserManager<User> userManager, RoleManager<Role> roleManager,
        IGroupService groupService, IEntityFactory entityFactory, IFractionService fractionService, IDb db)
    {
        _logger = logger;
        _elementByStringIdCollection = elementByStringIdCollection;
        _serverFilesProvider = serverFilesProvider;
        _userManager = userManager;
        _roleManager = roleManager;
        _groupService = groupService;
        _entityFactory = entityFactory;
        _fractionService = fractionService;
        _db = db;
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
            if(Enum.IsDefined(typeof(BlipIcon), pair.Value.Icon))
            {
                var entity = _entityFactory.CreateBlip((BlipIcon)pair.Value.Icon, pair.Value.Position, pair.Key);
                AssignElementToId(entity, pair.Key);
                _logger.LogInformation("Seeder: Created blip of id {elementId} with icon {blipIcon} at position {position}", pair.Key, pair.Value.Icon, pair.Value.Position);
            }
            else
            {
                _logger.LogError("Seeder: Failed to create blip of id {id} with icon {blipIcon} at position {position}", pair.Key, pair.Value.Icon, pair.Value.Position);
            }
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
            _logger.LogInformation("Seeder: Created pickup of id {elementId} with icon {pickupModel} at {position}", pair.Key, pair.Value.Model, pair.Value.Position);
        }
    }
    
    private void BuildMarkers(Dictionary<string, MarkerSeedData> markers)
    {
        foreach (var pair in markers)
        {
            if (Enum.IsDefined(typeof(MarkerType), pair.Value.MarkerType))
            {
                var entity = _entityFactory.CreateMarker(pair.Value.MarkerType, pair.Value.Position, 0, 0, pair.Key);
                AssignElementToId(entity, pair.Key);
                _logger.LogInformation("Seeder: Created marker of id {elementId} at {position}", pair.Key, pair.Value.Position);
            }
            else
                _logger.LogInformation("Seeder: Failed to create type {markerType} at {position}", pair.Value.MarkerType, pair.Value.Position);
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
                    await _groupService.AddMember(group.name, _createdAccounts[item.Key].Id, item.Value.Rank, item.Value.RankName);
                }
                catch(Exception) // Maybe member is already in group
                {

                }
            }
            if(created)
                _logger.LogInformation("Seeder: Created group {elementId} with members {members}", pair.Key, pair.Value.Members.Select(x => x.Key));
            else
                _logger.LogInformation("Seeder: Updated group {elementId} with members {members}", pair.Key, pair.Value.Members.Select(x => x.Key));
        }
    }

    private async Task BuildIdentityRoles(List<string> roles)
    {
        var existingRoles = await _roleManager.Roles.ToListAsync();
        foreach (var roleName in roles)
        {
            if (!existingRoles.Any(x => x.Name == roleName))
            {
                await _roleManager.CreateAsync(new Role
                {
                    Name = roleName
                });
                _logger.LogInformation("Seeder: Created role {roleName}", roleName);
            }
            else
                _logger.LogInformation("Seeder: Role {roleName} already exists", roleName);
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
                    _logger.LogInformation("Seeder: Created user {userName}", pair.Key);
                }
            }
            else
                _logger.LogInformation("Seeder: User {userName} already exists", pair.Key);

            if (user == null)
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

            _createdAccounts.Add(pair.Key, user);
        }
    }

    private async Task BuildFractions(Dictionary<string, FractionSeedData> fractions)
    {
        foreach (var fraction in fractions)
        {
            var id = fraction.Value.Id;
            var fractionData = await _db.Fractions
                .Include(x => x.Members)
                .Where(x => x.Id == id && x.Code == fraction.Value.Code && x.Name == fraction.Key).FirstOrDefaultAsync();

            if(fractionData == null)
            {
                var oldFractionData = await _db.Fractions
                    .Include(x => x.Members)
                    .Where(x => x.Id == id).FirstOrDefaultAsync();
                if(oldFractionData != null)
                {
                    _db.Fractions.Remove(oldFractionData);
                    _logger.LogInformation("Seeder: Removed old fraction '{fractionName}' from database.", oldFractionData.Name);
                }

                fractionData = new Fraction
                {
                    Id = id,
                    Name = fraction.Key,
                    Code = fraction.Value.Code,
                    Members = fraction.Value.Members.Select(x => new FractionMember
                    {
                        UserId = _createdAccounts[x.Key].Id,
                        Rank = x.Value.Rank,
                        RankName = x.Value.RankName,
                    }).ToList()
                };
                _db.Fractions.Add(fractionData);

                _logger.LogInformation("Seeder: Added fraction '{fractionName}' to database with id {fractionId}.", fractionData.Name);
            }

            _fractionService.CreateFraction(id, fraction.Key, fraction.Value.Code, fraction.Value.Position);
            foreach (var member in fraction.Value.Members)
            {
                _fractionService.InternalAddMember(id, _createdAccounts[member.Key].Id, member.Value.Rank, member.Value.RankName);
            }
            _logger.LogInformation("Seeder: Created fraction '{fractionName}' with id {fractionId}.", fractionData.Name);
        }
        await _db.SaveChangesAsync();
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
        await BuildFractions(seedData.Fractions);
        BuildBlips(seedData.Blips);
        BuildPickups(seedData.Pickups);
        BuildMarkers(seedData.Markers);
        await BuildGroups(seedData.Groups);
        _createdAccounts.Clear();
    }
}