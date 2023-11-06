namespace RealmCore.Tests.Tests.Components;

public class UserComponentTests
{
    [InlineData(new string[] { }, false)]
    [InlineData(new string[] { "Admin" }, true)]
    [Theory]
    public async Task UserShouldAuthorize(string[] roles, bool expectedAuthorized)
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        await realmTestingServer.SignInPlayer(player, roles);
        var usersService = player.GetRequiredService<IUsersService>();
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
        var user = player.User;
        user.SetSetting(1, "foo");
        bool hasSetting = user.TryGetSetting(1, out var settingValue);
        string? gotSettingValue = user.GetSetting(1);
        bool removedSetting = user.RemoveSetting(1);
        string? gotSettingAfterRemove = user.GetSetting(1);
        #endregion

        #region Assert
        hasSetting.Should().BeTrue();
        settingValue.Should().Be("foo");
        gotSettingValue.Should().Be("foo");
        removedSetting.Should().BeTrue();
        gotSettingAfterRemove.Should().BeNull();
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
        var user = player.User;
        var hasSomeUpgrade1 = user.HasUpgrade(1);
        var added1 = user.TryAddUpgrade(1);
        var added2 = user.TryAddUpgrade(1);
        var hasSomeUpgrade2 = user.HasUpgrade(1);
        var removed1 = user.TryRemoveUpgrade(1);
        var removed2 = user.TryRemoveUpgrade(1);
        var hasSomeUpgrade3 = user.HasUpgrade(1);
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
