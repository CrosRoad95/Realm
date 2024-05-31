namespace RealmCore.Tests.Integration.Players;

public class UserTests
{
    [Fact]
    public async Task TestLogInFlow()
    {
        #region Arrange
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer(true);

        var usersService = player.GetRequiredService<IUsersService>();
        var userManager = player.GetRequiredService<UserManager<UserData>>();
        var login = Guid.NewGuid().ToString()[..8];
        var password = "asdASD123!@#";
        #endregion

        #region Act
        var registerResult = await usersService.Register(login, password);
        var user = await player.GetRequiredService<IPlayerUserService>().GetUserByUserName(login) ?? throw new Exception("User not found");

        var validPassword = await userManager.CheckPasswordAsync(user, password);
        var logIn = async () => await usersService.LogIn(player, user);

        (await logIn()).Value.Should().BeOfType<UsersResults.LoggedIn>();
        player.User.IsLoggedIn.Should().BeTrue();
        (await logIn()).Value.Should().BeOfType<UsersResults.PlayerAlreadyLoggedIn>();

        var userId = registerResult.Match(registered =>
        {
            return registered.id;
        }, failedToLogin =>
        {
            throw new Exception("Failed to login");
        });


        var lastNick = await userManager.GetLastNickName(userId);
        #endregion

        #region Assert
        validPassword.Should().BeTrue();
        lastNick.Should().StartWith("TestPlayer");
        player.User.IsLoggedIn.Should().BeTrue();
        player.UserId.Should().Be(userId);
        #endregion
    }
}
