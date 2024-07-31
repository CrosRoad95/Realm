using static RealmCore.Server.Modules.Seeder.SeedData;

namespace RealmCore.Server.Modules.Seeder;

internal sealed class SeederServerBuilder : IDisposable
{
    private const string _basePath = "Seed";
    private readonly IServerFilesProvider _serverFilesProvider;
    private readonly UserManager<UserData> _userManager;
    private readonly IFractionsService _fractionService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IPlayerUserService _playerUserService;
    private readonly IServiceScope _serviceScope;
    private readonly Dictionary<string, ISeederProvider> _seederProviders = [];
    private readonly Dictionary<string, IAsyncSeederProvider> _asyncSeederProviders = [];
    private readonly ILogger<SeederServerBuilder> _logger;
    private readonly Dictionary<string, UserData> _createdUsers = [];
    private bool _isUpToDate = true;

    public SeederServerBuilder(ILogger<SeederServerBuilder> logger,
        IServerFilesProvider serverFilesProvider, UserManager<UserData> userManager, IFractionsService fractionService, IEnumerable<ISeederProvider> seederProviders, IEnumerable<IAsyncSeederProvider> asyncSeederProviders, IGroupRepository groupRepository, IServiceProvider serviceProvider, IPlayerUserService playerUserService)
    {
        _logger = logger;
        _serverFilesProvider = serverFilesProvider;
        _userManager = userManager;
        _fractionService = fractionService;
        _serviceProvider = serviceProvider;
        _playerUserService = playerUserService;
        _serviceScope = serviceProvider.CreateScope();
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

    private async Task BuildIdentityRoles(Dictionary<string, object> roles)
    {
        using var scope = _serviceProvider.CreateScope();
        using var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<RoleData>>();
        var existingRoles = await roleManager.Roles.ToListAsync();
        foreach (var roleName in roles)
        {
            if (!existingRoles.Any(x => x.Name == roleName.Key))
            {
                await roleManager.CreateAsync(new RoleData
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
            var user = await _playerUserService.GetUserByUserName(pair.Key);

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

            var allClaims = await _userManager.GetClaimsAsync(user);
            var claimsToRemove = allClaims.Where(x => claims.Any(y => y.Type == x.Type && y.Value != x.Value)).ToList();
            var claimsToAdd = claims.Where(x => !allClaims.Any(y => y.Type == x.Type)).ToList();

            if (claimsToRemove.Count > 0)
            {
                await _userManager.RemoveClaimsAsync(user, claimsToRemove);
                foreach (var claimToRemove in claimsToRemove)
                {
                    claimsToAdd.Add(claims.Where(x => x.Type == claimToRemove.Type).First());
                }
            }

            if(claimsToAdd.Count > 0)
                await _userManager.AddClaimsAsync(user, claimsToAdd);

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
                    {
                        if(user.DiscordIntegration.DiscordUserId != discordUserId)
                        {
                            user.DiscordIntegration.DiscordUserId = discordUserId;
                            _logger.LogInformation("Seeder: Updated discord integration with discord user id {discordUserId} for user {userName}", discordUserId, pair.Key);
                            _isUpToDate = false;
                        }
                    }
                    else
                    {
                        user.DiscordIntegration = new DiscordIntegrationData { DiscordUserId = discordUserId };
                        _logger.LogInformation("Seeder: Added discord integration with discord user id {discordUserId} for user {userName}", discordUserId, pair.Key);
                        _isUpToDate = false;
                    }
                }
            }

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
                    try
                    {
                        await _fractionService.AddMember(id, userId, member.Value.Rank, member.Value.RankName);
                        _logger.LogInformation("Seeder: Added member {userId} with rank: {fractionRank} ({fractionRankName}) to the fraction with id {fractionId}.", userId, member.Value.Rank, member.Value.RankName, id);
                        _isUpToDate = false;
                    }
                    catch (Exception)
                    {
                        // Ignore
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
        _createdUsers.Clear();
        if (_isUpToDate)
        {
            _logger.LogInformation("Seeder: Everything is up to date.");
        }
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}