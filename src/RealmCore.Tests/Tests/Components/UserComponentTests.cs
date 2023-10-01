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
}
