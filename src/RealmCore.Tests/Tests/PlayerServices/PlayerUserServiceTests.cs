namespace RealmCore.Tests.Tests.PlayerServices;

public class PlayerUserServiceTests
{
    [InlineData(new string[] { }, false)]
    [InlineData(new string[] { "Admin" }, true)]
    [Theory]
    public async Task UserShouldAuthorize(string[] roles, bool expectedAuthorized)
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        await realmTestingServer.SignInPlayer(player);
        var usersService = player.GetRequiredService<IUsersService>();
        foreach (var roleName in roles)
        {
            var roleManager = player.GetRequiredService<RoleManager<RoleData>>();
            await roleManager.CreateAsync(new RoleData
            {
                Name = roleName
            });
            await usersService.AddToRole(player, roleName);
        }
        #endregion

        #region Act
        var authorized = await usersService.AuthorizePolicy(player, "Admin");
        #endregion

        #region Assert
        authorized.Should().Be(expectedAuthorized);
        var isInCache = player.User.HasAuthorizedPolicy("Admin", out bool cacheAuthorized);
        isInCache.Should().BeTrue();
        cacheAuthorized.Should().Be(expectedAuthorized);
        #endregion
    }

    [Fact]
    public async Task ChangingRoleShouldClearPolicyAuthorizedCache()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        await realmTestingServer.SignInPlayer(player);
        var user = player.User;
        var usersService = player.GetRequiredService<IUsersService>();
        #endregion

        #region Act
        var authorized = await usersService.AuthorizePolicy(player, "Admin");
        #endregion

        #region Act & Assert
        authorized.Should().Be(false);
        var isInCache = user.HasAuthorizedPolicy("Admin", out bool cacheAuthorized);
        isInCache.Should().BeTrue();
        cacheAuthorized.Should().Be(false);

        user.AddRole("Admin");
        isInCache = user.HasAuthorizedPolicy("Admin", out bool _);
        isInCache.Should().BeFalse();
        #endregion
    }

    [Fact]
    public async Task SettingsShouldWork()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        await realmTestingServer.SignInPlayer(player);
        #endregion

        #region Act
        var settings = player.Settings;
        settings.Set(1, "foo");
        bool hasSetting = settings.TryGet(1, out var settingValue);
        string gotSettingValue = settings.Get(1);
        bool removedSetting = settings.Remove(1);
        bool exists = settings.Has(1);
        #endregion

        #region Assert
        hasSetting.Should().BeTrue();
        settingValue.Should().Be("foo");
        gotSettingValue.Should().Be("foo");
        removedSetting.Should().BeTrue();
        exists.Should().BeFalse();
        #endregion
    }

    [Fact]
    public async Task UpgradesShouldWork()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        await realmTestingServer.SignInPlayer(player);
        #endregion

        #region Act
        var upgrades = player.Upgrades;
        var hasSomeUpgrade1 = upgrades.Has(1);
        var added1 = upgrades.TryAdd(1);
        var added2 = upgrades.TryAdd(1);
        var hasSomeUpgrade2 = upgrades.Has(1);
        var removed1 = upgrades.TryRemove(1);
        var removed2 = upgrades.TryRemove(1);
        var hasSomeUpgrade3 = upgrades.Has(1);
        #endregion

        #region Assert
        hasSomeUpgrade1.Should().BeFalse();
        added1.Should().BeTrue();
        added2.Should().BeFalse();
        hasSomeUpgrade2.Should().BeTrue();
        removed1.Should().BeTrue();
        removed2.Should().BeFalse();
        hasSomeUpgrade3.Should().BeFalse();
        #endregion
    }
}
