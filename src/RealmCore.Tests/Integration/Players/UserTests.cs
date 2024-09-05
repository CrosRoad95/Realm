namespace RealmCore.Tests.Integration.Players;

public class UserTests : IClassFixture<RealmTestingServerHostingFixtureWithUniquePlayer>
{
    private readonly RealmTestingServerHostingFixtureWithUniquePlayer _fixture;
    private readonly RealmTestingServerHosting<RealmTestingPlayer> _hosting;
    private readonly RealmTestingPlayer _player;

    public UserTests(RealmTestingServerHostingFixtureWithUniquePlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _hosting = fixture.Hosting;
    }

    [Fact]
    public async Task TestLogInFlow()
    {
        var player = await _hosting.CreatePlayer(true);

        var usersService = player.GetRequiredService<UsersService>();
        var userManager = player.GetRequiredService<UserManager<UserData>>();
        var login = Guid.NewGuid().ToString()[..8];
        var password = "asdASD123!@#";

        var registerResult = await usersService.Register(login, password);
        var user = await player.GetRequiredService<UsersService>().GetUserByUserName(login) ?? throw new Exception("User not found");

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

        var userDataRepository = player.GetRequiredService<UsersRepository>();
        var lastNick = await userDataRepository.GetLastNickName(userId);

        validPassword.Should().BeTrue();
        //lastNick.Should().StartWith("TestPlayer");
        player.User.IsLoggedIn.Should().BeTrue();
        player.UserId.Should().Be(userId);
    }
}
