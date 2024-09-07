namespace RealmCore.Tests.Integration;

public class UsersServiceTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingPlayer _player;
    private readonly PlayerGroupsFeature _groups;
    private readonly UsersService _usersService;

    public UsersServiceTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _groups = _player.Groups;
        _usersService = _fixture.Hosting.GetRequiredService<UsersService>();
    }

    [Fact]
    public async Task GettingUserByIdShouldWork()
    {
        var user = await _usersService.GetUserById(_player.UserId);
        user.Should().NotBeNull();
    }
}