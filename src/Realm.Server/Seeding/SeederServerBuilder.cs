using static Realm.Server.Seeding.SeedData;

namespace Realm.Server.Seeding;

internal sealed class SeederServerBuilder
{
    private const string _basePath = "Seed";
    private readonly IServerFilesProvider _serverFilesProvider;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IGroupService _groupService;
    private readonly IEntityFactory _entityFactory;
    private readonly IFractionService _fractionService;
    private readonly Dictionary<string, ISeederProvider> _seederProviders = new();
    private readonly Dictionary<string, IAsyncSeederProvider> _asyncSeederProviders = new();
    private readonly ILogger<SeederServerBuilder> _logger;
    private readonly IECS _ecs;
    private readonly Dictionary<string, User> _createdAccounts = new();
    public SeederServerBuilder(ILogger<SeederServerBuilder> logger, IECS ecs,
        IServerFilesProvider serverFilesProvider, UserManager<User> userManager, RoleManager<Role> roleManager,
        IGroupService groupService, IEntityFactory entityFactory, IFractionService fractionService, IEnumerable<ISeederProvider> seederProviders,
        IEnumerable<IAsyncSeederProvider> asyncSeederProviders)
    {
        _logger = logger;
        _ecs = ecs;
        _serverFilesProvider = serverFilesProvider;
        _userManager = userManager;
        _roleManager = roleManager;
        _groupService = groupService;
        _entityFactory = entityFactory;
        _fractionService = fractionService;
        foreach (var seederProvider in seederProviders)
        {
            _seederProviders[seederProvider.SeedKey] = seederProvider;
            logger.LogInformation("Using {seederProvider} for seed key {seedKey}", seederProvider, seederProvider.SeedKey);
        }

        foreach (var seederProvider in asyncSeederProviders)
        {
            _asyncSeederProviders[seederProvider.SeedKey] = seederProvider;
            logger.LogInformation("Using async {seederProvider} for seed key {seedKey}", seederProvider, seederProvider.SeedKey);
        }
    }

    private void BuildBlips(Dictionary<string, BlipSeedData> blips)
    {
        foreach (var pair in blips)
        {
            if(Enum.IsDefined(typeof(BlipIcon), pair.Value.Icon))
            {
                var entity = _entityFactory.CreateBlip((BlipIcon)pair.Value.Icon, pair.Value.Position, new ConstructionInfo
                {
                    Id = pair.Key
                });
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
            var entity = _entityFactory.CreatePickup(pair.Value.Model, pair.Value.Position, new ConstructionInfo
            {
                Id = pair.Key,
            });
            if (pair.Value.Text3d != null)
            {
                entity.AddComponent(new Text3dComponent(pair.Value.Text3d, new Vector3(0, 0, 0.75f)));
            }
            _logger.LogInformation("Seeder: Created pickup of id {elementId} with icon {pickupModel} at {position}", pair.Key, pair.Value.Model, pair.Value.Position);
        }
    }
    
    private void BuildMarkers(Dictionary<string, MarkerSeedData> markers)
    {
        foreach (var pair in markers)
        {
            if (Enum.IsDefined(typeof(MarkerType), pair.Value.MarkerType))
            {
                var entity = _entityFactory.CreateMarker(pair.Value.MarkerType, pair.Value.Position,new ConstructionInfo
                {
                    Id = pair.Key,
                });
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
            Domain.Concepts.Group group;
            if(!await _groupService.GroupExistsByNameOrShortut(pair.Key, pair.Value.Shortcut))
            {
                group = await _groupService.CreateGroup(pair.Key, pair.Value.Shortcut, pair.Value.GroupKind);
                _logger.LogInformation("Seeder: Created group {elementId} with members {members}", pair.Key, pair.Value.Members.Select(x => x.Key));
            }
            else
            {
                group = await _groupService.GetGroupByNameOrShortut(pair.Key, pair.Value.Shortcut) ?? throw new Exception("Failed to get group by name or shortcut");
                _logger.LogInformation("Seeder: Updated group {elementId} with members {members}", pair.Key, pair.Value.Members.Select(x => x.Key));
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
        }
    }

    private async Task BuildIdentityRoles(Dictionary<string, object> roles)
    {
        var existingRoles = await _roleManager.Roles.ToListAsync();
        foreach (var roleName in roles)
        {
            if (!existingRoles.Any(x => x.Name == roleName.Key))
            {
                await _roleManager.CreateAsync(new Role
                {
                    Name = roleName.Key
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
            var user = await _userManager.Users
                .Include(x => x.DiscordIntegration)
                .Where(x => x.UserName == pair.Key)
                .FirstOrDefaultAsync();

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
                    new("persistent", "true"),
                });

            await _userManager.RemoveClaimsAsync(user, await _userManager.GetClaimsAsync(user));
            await _userManager.AddClaimsAsync(user, claims);
            await _userManager.AddToRolesAsync(user, pair.Value.Roles);

            var integrations = pair.Value.Integrations;
            if(integrations != null)
            {
                if(integrations.Discord != null)
                {
                    var discordUserId = integrations.Discord.UserId;
                    if(user.DiscordIntegration != null)
                        user.DiscordIntegration.DiscordUserId = discordUserId;
                    else
                        user.DiscordIntegration = new DiscordIntegration { DiscordUserId = discordUserId };
                    _logger.LogInformation("Seeder: Added discord integration with discord user id {discordUserId} for user {userName}", discordUserId, pair.Key);
                }
                await _userManager.UpdateAsync(user);
            }
            _createdAccounts.Add(pair.Key, user);
        }
    }

    private async Task BuildFractions(Dictionary<string, FractionSeedData> fractions)
    {
        foreach (var fraction in fractions)
        {
            var id = fraction.Value.Id;
            await _fractionService.InternalCreateFraction(id, fraction.Key, fraction.Value.Code, fraction.Value.Position);

            foreach (var member in fraction.Value.Members)
            {
                var userId = _createdAccounts[member.Key].Id;
                if (!_fractionService.HasMember(id, userId))
                {
                    if(await _fractionService.TryAddMember(id, userId, member.Value.Rank, member.Value.RankName))
                        _logger.LogInformation("Seeder: Added member {userId} with rank: {fractionRank} ({fractionRankName}) to the fraction with id {fractionId}.", userId, member.Value.Rank, member.Value.RankName, id);
                }
            }
            _logger.LogInformation("Seeder: Created fraction '{fractionCode}' with id {fractionId}.", fraction.Value.Code, id);
        }
    }
    
    public async Task Build()
    {
        var result = new JObject();
        List<SeedData> seedDatas = new();
        var seedFiles = _serverFilesProvider.GetFiles(_basePath).ToList();
        _logger.LogInformation("Found seed files: {seedFiles}", seedFiles);
        foreach (var seedFileName in seedFiles)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<SeedData>(File.ReadAllText(seedFileName));
                if (data == null)
                    throw new Exception("Something went wrong while deserializing.");
                seedDatas.Add(data);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to load seed file: {seedFileName}", seedFileName);
            }
        }

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
        await BuildFrom(seedData);

        var seedKeyValuePairs = result.ToObject<Dictionary<string, Dictionary<string, JObject>>>();
        foreach (var seedKeyValuePair in seedKeyValuePairs)
        {
            if (_seederProviders.TryGetValue(seedKeyValuePair.Key, out var value))
            {
                foreach (var keyValuePair in seedKeyValuePair.Value)
                {
                    value.Seed(seedKeyValuePair.Key, keyValuePair.Key, keyValuePair.Value);
                }
            }
        }

        foreach (var seedKeyValuePair in seedKeyValuePairs)
        {
            if (_asyncSeederProviders.TryGetValue(seedKeyValuePair.Key, out var value))
            {
                foreach (var keyValuePair in seedKeyValuePair.Value)
                {
                    await value.SeedAsync(seedKeyValuePair.Key, keyValuePair.Key, keyValuePair.Value);
                }
            }
        }
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