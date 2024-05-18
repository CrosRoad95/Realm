namespace RealmCore.Tests.Integration.Players;

[Collection("IntegrationTests")]
public class UserTests : RealmRemoteDatabaseIntegrationTestingBase
{
    [Fact]
    public async Task TestLogInFlow()
    {
        #region Arrange
        var server = await CreateServerAsync();
        var player = await CreatePlayerAsync(false);

        var usersService = player.GetRequiredService<IUsersService>();
        var userManager = player.GetRequiredService<UserManager<UserData>>();
        var login = Guid.NewGuid().ToString()[..8];
        var password = "asdASD123!@#";
        #endregion

        #region Act
        var registerResult = await usersService.Register(login, password);
        var user = await player.GetRequiredService<IPlayerUserService>().GetUserByUserName(login, server.DateTimeProvider.Now) ?? throw new Exception("User not found");

        var validPassword = await userManager.CheckPasswordAsync(user, password);
        var signIn = async () => await usersService.LogIn(player, user);

        await signIn.Should().NotThrowAsync();
        player.User.IsLoggedIn.Should().BeTrue();
        await signIn.Should().ThrowAsync<UserAlreadySignedInException>();

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
