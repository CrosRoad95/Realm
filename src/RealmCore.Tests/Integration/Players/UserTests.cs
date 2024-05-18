namespace RealmCore.Tests.Integration.Players;

[Collection("IntegrationTests")]
public class UserTests : RealmRemoteDatabaseIntegrationTestingBase
{
    [Fact]
    public async Task TestSignInFlow()
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
        var userId = await usersService.SignUp(login, password);
        var user = await player.GetRequiredService<IPlayerUserService>().GetUserByUserName(login, server.DateTimeProvider.Now) ?? throw new Exception("User not found");

        var validPassword = await userManager.CheckPasswordAsync(user, password);
        var signIn = async () => await usersService.SignIn(player, user);

        await signIn.Should().NotThrowAsync();
        player.User.IsSignedIn.Should().BeTrue();
        await signIn.Should().ThrowAsync<UserAlreadySignedInException>();
        var lastNick = await userManager.GetLastNickName(userId);
        #endregion

        #region Assert
        validPassword.Should().BeTrue();
        lastNick.Should().StartWith("TestPlayer");
        player.User.IsSignedIn.Should().BeTrue();
        player.PersistentId.Should().Be(userId);
        #endregion
    }
}
