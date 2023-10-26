using Microsoft.AspNetCore.Identity;
using RealmCore.Persistence.Data;

namespace RealmCore.Tests.Tests.Services;

public class UsersManagerTests
{
    private readonly EntityHelper _entityHelper;
    private readonly RealmTestingServer _realmTestingServer;
    private readonly IUsersService _signInService;
    private readonly UserManager<UserData> _userManager;

    public UsersManagerTests()
    {
        _realmTestingServer = new();
        _entityHelper = new(_realmTestingServer);

        _signInService = _realmTestingServer.GetRequiredService<IUsersService>();
        _userManager = _realmTestingServer.GetRequiredService<UserManager<UserData>>();
    }

    [Fact]
    public async Task TestSignInFlow()
    {
        #region Arrange
        var login = Guid.NewGuid().ToString()[..8];
        var password = "asdASD123!@#";
        var playerEntity = _entityHelper.CreatePlayerEntity();
        #endregion

        #region Act
        var userId = await _signInService.SignUp(login, password);
        var user = await _userManager.GetUserByLogin(login) ?? throw new Exception("User not found");

        var validPassword = await _userManager.CheckPasswordAsync(user, password);
        var signedIn = await _signInService.SignIn(playerEntity, user);
        var lastNick = await _userManager.GetLastNickName(userId);
        #endregion

        #region Assert
        validPassword.Should().BeTrue();
        signedIn.Should().BeTrue();
        lastNick.Should().Be("CrosRoad95");
        #endregion
    }

    [Fact]
    public async Task SignInShouldNotAddComponentsWhenFailedToSignIn()
    {
        #region Arrange
        var login = Guid.NewGuid().ToString()[..8];
        var password = "asdASD123!@#";
        var playerEntity = _entityHelper.CreatePlayerEntity(true);
        #endregion

        #region Act
        var wasComponentCount = 0;
        _signInService.SignedIn += e =>
        {
            wasComponentCount = e.ComponentsCount;
            throw new Exception();
        };

        var userId = await _signInService.SignUp(login, password);
        var user = await _userManager.GetUserByLogin(login) ?? throw new Exception("User not found");

        var signedIn = await _signInService.SignIn(playerEntity, user);
        #endregion

        #region Assert
        signedIn.Should().BeFalse();
        playerEntity.Components.Should().HaveCount(3);
        wasComponentCount.Should().Be(16);
        #endregion
    }

    [InlineData("CrosRoad95", true)]
    [InlineData("CrosRoad69", false)]
    [Theory]
    public void TryGetPlayerByNameTests(string nick, bool shouldExists)
    {
        #region Arrange
        var playerEntity = _entityHelper.CreatePlayerEntity();
        #endregion

        #region Act
        bool found = _signInService.TryGetPlayerByName(nick, out var foundPlayerEntity);
        #endregion

        #region Assert
        if (shouldExists)
        {
            found.Should().BeTrue();
            (playerEntity == foundPlayerEntity).Should().BeTrue();
        }
        else
        {
            found.Should().BeFalse();
            foundPlayerEntity.Should().BeNull();
        }
        #endregion
    }

    [InlineData("road", true)]
    [InlineData("asd", false)]
    [Theory]
    public void SearchPlayersByNameTests(string pattern, bool shouldExists)
    {
        #region Arrange
        var playerEntity = _entityHelper.CreatePlayerEntity();
        #endregion

        #region Act
        var found = _signInService.SearchPlayersByName(pattern);
        #endregion

        #region Assert
        if (shouldExists)
        {
            var foundPlayerEntity = found.First();
            (playerEntity == foundPlayerEntity).Should().BeTrue();
        }
        else
        {
            found.Should().BeEmpty();
        }
        #endregion
    }
}
