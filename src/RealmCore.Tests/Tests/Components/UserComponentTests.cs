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
        var authorized = await usersService.AuthorizePolicy(player.GetRequiredComponent<UserComponent>(), "Admin");
        #endregion

        #region Assert
        authorized.Should().Be(expectedAuthorized);
        var isInCache = player.GetRequiredComponent<UserComponent>().HasAuthorizedPolicy("Admin", out bool cacheAuthorized);
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
        var userComponent = player.GetRequiredComponent<UserComponent>();
        var usersService = player.GetRequiredService<IUsersService>();
        #endregion

        #region Act
        var authorized = await usersService.AuthorizePolicy(userComponent, "Admin");
        #endregion

        #region Acy & Assert
        authorized.Should().Be(false);
        var isInCache = userComponent.HasAuthorizedPolicy("Admin", out bool cacheAuthorized);
        isInCache.Should().BeTrue();
        cacheAuthorized.Should().Be(false);

        userComponent.AddRole("Admin");
        isInCache = userComponent.HasAuthorizedPolicy("Admin", out bool _);
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
        var userComponent = player.GetRequiredComponent<UserComponent>();
        #endregion

        #region Act
        userComponent.SetSetting(1, "foo");
        bool hasSetting = userComponent.TryGetSetting(1, out var settingValue);
        string? gotSettingValue = userComponent.GetSetting(1);
        bool removedSetting = userComponent.RemoveSetting(1);
        string? gotSettingAfterRemove = userComponent.GetSetting(1);
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
        var userComponent = player.GetRequiredComponent<UserComponent>();
        #endregion

        #region Act
        var hasSomeUpgrade1 = userComponent.HasUpgrade(1);
        var added1 = userComponent.TryAddUpgrade(1);
        var added2 = userComponent.TryAddUpgrade(1);
        var hasSomeUpgrade2 = userComponent.HasUpgrade(1);
        var removed1 = userComponent.TryRemoveUpgrade(1);
        var removed2 = userComponent.TryRemoveUpgrade(1);
        var hasSomeUpgrade3 = userComponent.HasUpgrade(1);
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
