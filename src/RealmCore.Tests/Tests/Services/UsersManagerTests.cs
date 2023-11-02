namespace RealmCore.Tests.Tests.Services;

public class UsersManagerTests
{
    [Fact]
    public async Task TestSignInFlow()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer(true);
        var usersService = player.GetRequiredService<IUsersService>();
        var userManager = player.GetRequiredService<UserManager<UserData>>();
        var login = Guid.NewGuid().ToString()[..8];
        var password = "asdASD123!@#";
        #endregion

        #region Act
        var userId = await usersService.SignUp(login, password);
        var user = await userManager.GetUserByLogin(login) ?? throw new Exception("User not found");

        var validPassword = await userManager.CheckPasswordAsync(user, password);
        var signedIn = await usersService.SignIn(player, user);
        var lastNick = await userManager.GetLastNickName(userId);
        #endregion

        #region Assert
        validPassword.Should().BeTrue();
        signedIn.Should().BeTrue();
        lastNick.Should().Be("CrosRoad95");
        player.IsLoggedIn.Should().BeTrue();
        player.UserId.Should().Be(userId);
        #endregion
    }

    [Fact]
    public async Task SignInShouldNotAddComponentsWhenFailedToSignIn()
    {
        #region Arrange
        var login = Guid.NewGuid().ToString()[..8];
        var password = "asdASD123!@#";

        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer(false);
        var usersService = player.GetRequiredService<IUsersService>();
        var userManager = player.GetRequiredService<UserManager<UserData>>();
        #endregion

        #region Act
        usersService.SignedIn += e =>
        {
            throw new Exception();
        };

        var userId = await usersService.SignUp(login, password);
        var user = await userManager.GetUserByLogin(login) ?? throw new Exception("User not found");

        var signedIn = await usersService.SignIn(player, user);
        #endregion

        #region Assert
        signedIn.Should().BeFalse();
        player.Components.ComponentsList.Should().BeEmpty();
        #endregion
    }

    [InlineData("CrosRoad95", true)]
    [InlineData("CrosRoad69", false)]
    [Theory]
    public void TryGetPlayerByNameTests(string nick, bool shouldExists)
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer(false);
        var usersService = player.GetRequiredService<IUsersService>();
        #endregion

        #region Act
        bool found = usersService.TryGetPlayerByName(nick, out var foundPlayerEntity);
        #endregion

        #region Assert
        if (shouldExists)
        {
            found.Should().BeTrue();
            (player == foundPlayerEntity).Should().BeTrue();
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
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer(false);
        var usersService = player.GetRequiredService<IUsersService>();
        #endregion

        #region Act
        var found = usersService.SearchPlayersByName(pattern);
        #endregion

        #region Assert
        if (shouldExists)
        {
            var foundPlayerEntity = found.First();
            (player == foundPlayerEntity).Should().BeTrue();
        }
        else
        {
            found.Should().BeEmpty();
        }
        #endregion
    }
}
