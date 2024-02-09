using static RealmCore.Server.Modules.Seeder.SeedData;

namespace RealmCore.Server.Modules.Seeder;

internal sealed class SeederServerBuilder
{
    private const string _basePath = "Seed";
    private readonly IServerFilesProvider _serverFilesProvider;
    private readonly UserManager<UserData> _userManager;
    private readonly RoleManager<RoleData> _roleManager;
    private readonly IGroupService _groupService;
    private readonly IElementFactory _elementFactory;
    private readonly IFractionService _fractionService;
    private readonly IGroupRepository _groupRepository;
    private readonly Text3dService _text3dService;
    private readonly Dictionary<string, ISeederProvider> _seederProviders = [];
    private readonly Dictionary<string, IAsyncSeederProvider> _asyncSeederProviders = [];
    private readonly ILogger<SeederServerBuilder> _logger;
    private readonly Dictionary<string, UserData> _createdUsers = [];
    private bool _isUpToDate = true;

    public SeederServerBuilder(ILogger<SeederServerBuilder> logger,
        IServerFilesProvider serverFilesProvider, UserManager<UserData> userManager, RoleManager<RoleData> roleManager,
        IGroupService groupService, IElementFactory elementFactory, IFractionService fractionService, IEnumerable<ISeederProvider> seederProviders,
        IEnumerable<IAsyncSeederProvider> asyncSeederProviders, IGroupRepository groupRepository, Text3dService text3dService)
    {
        _logger = logger;
        _serverFilesProvider = serverFilesProvider;
        _userManager = userManager;
        _roleManager = roleManager;
        _groupService = groupService;
        _elementFactory = elementFactory;
        _fractionService = fractionService;
        _groupRepository = groupRepository;
        _text3dService = text3dService;
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
            if (Enum.IsDefined(typeof(BlipIcon), pair.Value.Icon))
            {

                var blip = _elementFactory.CreateBlip(pair.Value.Position, (BlipIcon)pair.Value.Icon, elementBuilder: e => e.ElementName = pair.Key);
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
            var pickup = _elementFactory.CreatePickup(pair.Value.Position, pair.Value.Model, elementBuilder: e => e.ElementName = pair.Key);
            if (pair.Value.Text3d != null)
            {
                _text3dService.CreateText3d(pickup.Position + new Vector3(0, 0, 0.75f), pair.Value.Text3d);
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
                var marker = _elementFactory.CreateMarker(pair.Value.Position, pair.Value.MarkerType, 1, pair.Value.Color, elementBuilder: e => e.ElementName = pair.Key);
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
            Group group;
            if (!await _groupService.GroupExistsByNameOrShorCut(pair.Key, pair.Value.Shortcut))
            {
                group = await _groupService.CreateGroup(pair.Key, pair.Value.Shortcut, pair.Value.GroupKind);
                _logger.LogInformation("Seeder: Created group {elementId} with members {members}", pair.Key, pair.Value.Members.Select(x => x.Key));
            }
            else
            {
                group = await _groupService.GetGroupByNameOrShortCut(pair.Key, pair.Value.Shortcut) ?? throw new Exception("Failed to get group by name or shortcut");
            }

            foreach (var item in pair.Value.Members)
            {
                if (!await _groupRepository.IsUserInGroup(group.id, _createdUsers[item.Key].Id))
                {
                    await _groupRepository.TryAddMember(group.id, _createdUsers[item.Key].Id, item.Value.Rank, item.Value.RankName);
                    _logger.LogInformation("Seeder: Updated group {elementId} with members {members}", pair.Key, pair.Value.Members.Select(x => x.Key));
                    _isUpToDate = false;
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
                await _roleManager.CreateAsync(new RoleData
                {
                    Name = roleName.Key
                });
                _logger.LogInformation("Seeder: Created role {roleName}", roleName);
                _isUpToDate = false;
            }
        }
    }

    private async Task BuildIdentityUsers(Dictionary<string, UserSeedData> users)
    {
        foreach (var pair in users)
        {
            var userSeedData = pair.Value;
            var user = await _userManager.GetUserByUserName(pair.Key);

            if (user == null)
            {
                var identityResult = await _userManager.CreateAsync(new UserData
                {
                    UserName = pair.Key,
                }, userSeedData.Password);
                if (identityResult.Succeeded)
                {
                    user = await _userManager.FindByNameAsync(pair.Key);
                    _logger.LogInformation("Seeder: Created user {userName}", pair.Key);
                    _isUpToDate = false;
                }
                else
                {
                    var identityResultError = string.Join(" ", identityResult.Errors.Select(x => x.Description));
                    _logger.LogError("Seeder: Failed to create user {userName}, reason: {identityResultError}", pair.Key, identityResultError);
                }
            }

            if (user == null)
                throw new Exception($"Failed to create user '{pair.Key}'");

            var claims = new List<Claim>
                {
                    new("seeded", "true"),
                    new("persistent", "true"),
                };
            if (userSeedData.Claims != null)
                claims = userSeedData.Claims.Select(x => new Claim(x.Key, x.Value)).Concat(claims).ToList();

            await _userManager.RemoveClaimsAsync(user, await _userManager.GetClaimsAsync(user));
            await _userManager.AddClaimsAsync(user, claims);
            if (pair.Value.Roles != null)
                await _userManager.AddToRolesAsync(user, pair.Value.Roles);

            if (pair.Value.Settings != null)
            {
                foreach (var item in pair.Value.Settings)
                {
                    if (!user.Settings.Any(x => x.SettingId == item.Key))
                    {
                        user.Settings.Add(new UserSettingData
                        {
                            SettingId = item.Key,
                            Value = item.Value,
                        });
                    }
                }
            }

            var integrations = pair.Value.Integrations;
            if (integrations != null)
            {
                if (integrations.Discord != null)
                {
                    var discordUserId = integrations.Discord.UserId;
                    if (user.DiscordIntegration != null)
                        user.DiscordIntegration.DiscordUserId = discordUserId;
                    else
                    {
                        user.DiscordIntegration = new DiscordIntegrationData { DiscordUserId = discordUserId };
                        _logger.LogInformation("Seeder: Added discord integration with discord user id {discordUserId} for user {userName}", discordUserId, pair.Key);
                        _isUpToDate = false;
                    }
                }
            }
            await _userManager.UpdateAsync(user);

            _createdUsers.Add(pair.Key, user);
        }
    }

    private async Task BuildFractions(Dictionary<string, FractionSeedData> fractions)
    {
        foreach (var fraction in fractions)
        {
            var id = fraction.Value.Id;
            if (await _fractionService.TryCreateFraction(id, fraction.Key, fraction.Value.Code, fraction.Value.Position))
            {
                _logger.LogInformation("Seeder: Created fraction of id {fractionId} name: {fractionName}, code: {fractionCode}.", id, fraction.Key, fraction.Value.Code);
                _isUpToDate = false;
            }

            foreach (var member in fraction.Value.Members)
            {
                var userId = _createdUsers[member.Key].Id;
                if (!_fractionService.HasMember(id, userId))
                {
                    if (await _fractionService.TryAddMember(id, userId, member.Value.Rank, member.Value.RankName))
                    {
                        _logger.LogInformation("Seeder: Added member {userId} with rank: {fractionRank} ({fractionRankName}) to the fraction with id {fractionId}.", userId, member.Value.Rank, member.Value.RankName, id);
                        _isUpToDate = false;
                    }
                }
            }
            _logger.LogInformation("Seeder: Created fraction '{fractionCode}' with id {fractionId}.", fraction.Value.Code, id);
        }
    }

    public async Task Build(CancellationToken cancellationToken = default)
    {
        var result = new JObject();
        List<SeedData> seedDataList = [];
        var seedFiles = _serverFilesProvider.GetFiles(_basePath).ToList();
        _logger.LogInformation("Found seed files: {seedFiles}", seedFiles);
        foreach (var seedFileName in seedFiles)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<SeedData>(await File.ReadAllTextAsync(seedFileName, cancellationToken)) ?? throw new Exception("Something went wrong while deserializing.");
                seedDataList.Add(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load seed file: {seedFileName}", seedFileName);
            }
        }

        foreach (var sourceObject in seedDataList)
        {
            var @object = JObject.Parse(JsonConvert.SerializeObject(sourceObject));
            result.Merge(@object, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            });
        }

        var seedData = result.ToObject<SeedData>() ?? throw new Exception("Failed to load seed data.");
        await BuildFrom(seedData);

        var seedKeyValuePairs = result.ToObject<Dictionary<string, Dictionary<string, JObject>>>();
        if (seedKeyValuePairs != null)
        {
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
    }

    private async Task BuildFrom(SeedData seedData)
    {
        await BuildIdentityRoles(seedData.Roles);
        await BuildIdentityUsers(seedData.Users);
        await BuildFractions(seedData.Fractions);
        BuildBlips(seedData.Blips);
        BuildPickups(seedData.Pickups);
        BuildMarkers(seedData.Markers);
        await BuildGroups(seedData.Groups);
        _createdUsers.Clear();
        if (_isUpToDate)
        {
            _logger.LogInformation("Seeder: Everything is up to date.");
        }
    }
}