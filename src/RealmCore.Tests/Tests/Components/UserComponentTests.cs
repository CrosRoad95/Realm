namespace RealmCore.Tests.Tests.Components;

public class UserComponentTests
{
    private readonly RealmTestingServer _server;
    private readonly EntityHelper _entityHelper;

    public UserComponentTests()
    {
        _server = new();
        _entityHelper = new(_server);
    }

    [InlineData(new string[] { }, false)]
    [InlineData(new string[] { "Admin" }, true)]
    [Theory]
    public async Task UserShouldAuthorize(string[] roles, bool expectedAuthorized)
    {
        #region Arrange

        var usersService = _server.GetRequiredService<IUsersService>();
        var player = _entityHelper.CreatePlayerEntity();
        var userComponent = await _entityHelper.LogInEntity(player, roles);
        #endregion

        #region Act
        var authorized = await usersService.AuthorizePolicy(userComponent, "Admin");
        #endregion

        #region Assert
        authorized.Should().Be(expectedAuthorized);
        var isInCache = userComponent.HasAuthorizedPolicy("Admin", out bool cacheAuthorized);
        isInCache.Should().BeTrue();
        cacheAuthorized.Should().Be(expectedAuthorized);
        #endregion
    }

    [Fact]
    public async Task ChangingRoleShouldClearPolicyAuthorizedCache()
    {
        #region Arrange
        var usersService = _server.GetRequiredService<IUsersService>();
        var player = _entityHelper.CreatePlayerEntity();
        var userComponent = await _entityHelper.LogInEntity(player);
        #endregion

        #region Act
        var authorized = await usersService.AuthorizePolicy(userComponent, "Admin");
        #endregion

        #region Acy & Assert
        authorized.Should().Be(false);
        var isInCache = userComponent.HasAuthorizedPolicy("Admin", out bool cacheAuthorized);
        isInCache.Should().BeTrue();
        cacheAuthorized.Should().Be(false);

        await userComponent.AddRole("Admin");
        isInCache = userComponent.HasAuthorizedPolicy("Admin", out bool _);
        isInCache.Should().BeFalse();
        #endregion
    }

    [Fact]
    public async Task SettingsShouldWork()
    {
        #region Arrange
        var player = _entityHelper.CreatePlayerEntity();
        var userComponent = await _entityHelper.LogInEntity(player);
        #endregion

        #region Acy
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
}
