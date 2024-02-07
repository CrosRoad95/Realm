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
        var user = await userManager.GetUserByUserName(login) ?? throw new Exception("User not found");

        var validPassword = await userManager.CheckPasswordAsync(user, password);
        var signedIn = await usersService.SignIn(player, user);
        var lastNick = await userManager.GetLastNickName(userId);
        #endregion

        #region Assert
        validPassword.Should().BeTrue();
        signedIn.Should().BeTrue();
        lastNick.Should().Be("CrosRoad95");
        player.IsSignedIn.Should().BeTrue();
        player.UserId.Should().Be(userId);
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
        bool found = usersService.TryGetPlayerByName(nick, out var foundPlayer);
        #endregion

        #region Assert
        if (shouldExists)
        {
            found.Should().BeTrue();
            (player == foundPlayer).Should().BeTrue();
        }
        else
        {
            found.Should().BeFalse();
            foundPlayer.Should().BeNull();
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
        var found = usersService.SearchPlayersByName(pattern, false);
        #endregion

        #region Assert
        if (shouldExists)
        {
            var foundPlayer = found.First();
            (player == foundPlayer).Should().BeTrue();
        }
        else
        {
            found.Should().BeEmpty();
        }
        #endregion
    }
}
