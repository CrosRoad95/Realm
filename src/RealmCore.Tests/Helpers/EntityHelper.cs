namespace RealmCore.Tests.Helpers;

internal class EntityHelper
{
    private readonly TestingServer _testingServer;
    private readonly IServiceProvider _serviceProvider;
    private readonly IElementFactory _elementFactory;
    private readonly IBanService _banService;

    public EntityHelper(TestingServer testingServer)
    {
        _testingServer = testingServer;
        _serviceProvider = _testingServer.GetRequiredService<IServiceProvider>();
        _elementFactory = _testingServer.GetRequiredService<IElementFactory>();
        _banService = _testingServer.GetRequiredService<IBanService>();
    }

    public RealmPlayer CreatePlayerEntity(bool withSerialAndIp = true)
    {
        var player = _testingServer.AddFakePlayer();

        if (withSerialAndIp)
        {
            player.Client = new FakeClient(player)
            {
                Serial = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                IPAddress = System.Net.IPAddress.Parse("127.0.0.1")
            };
        }

        player.Name = "CrosRoad95";
        player.TriggerResourceStarted(420);
        entity.AddComponent(new PlayerElementComponent(player, new Vector2(1920, 1080), new System.Globalization.CultureInfo("pl-PL")));

        return entity;
    }

    public async Task<UserComponent> LogInPlayer(RealmPlayer player, string[]? roles = null)
    {
        var claims = new List<Claim>
        {
            new("test", "true"),
        };
        var roleManager = _serviceProvider.GetRequiredService<RoleManager<RoleData>>();
        foreach (var roleName in new string[] { "Admin" })
        {
            await roleManager.CreateAsync(new RoleData
            {
                Name = roleName
            });
        }

        var userManager = _serviceProvider.GetRequiredService<UserManager<UserData>>();
        var signInManager = _serviceProvider.GetRequiredService<SignInManager<UserData>>();

        var user = new UserData
        {
            UserName = $"userName{Guid.NewGuid()}",
            Upgrades = new List<UserUpgradeData>(),
        };
        await userManager.CreateAsync(user);
        await userManager.AddClaimsAsync(user, claims);

        var claimsPrincipal = await signInManager.CreateUserPrincipalAsync(user);

        if (roles != null)
            foreach (var role in roles)
            {
                if (claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        var bans = await _banService.GetBansByUserIdAndSerial(user.Id, "AAAA");
        var userComponent = player.AddComponent(new UserComponent(user, claimsPrincipal, bans));
        return userComponent;
    }
}
