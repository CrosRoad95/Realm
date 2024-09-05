namespace RealmCore.Tests.Integration.Players;

public class PlayersTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingServerHosting<RealmTestingPlayer> _hosting;
    private readonly RealmTestingPlayer _player;

    public PlayersTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _hosting = fixture.Hosting;
    }

    //[InlineData(new string[] { }, false)]
    //[InlineData(new string[] { "SampleRole" }, true)]
    //[Theory]
    public async Task UserShouldAuthorize(string[] roles, bool expectedAuthorized)
    {
        var usersService = _player.GetRequiredService<UsersService>();
        foreach (var roleName in roles)
        {
            var roleManager = _player.GetRequiredService<RoleManager<RoleData>>();
            await roleManager.CreateAsync(new RoleData
            {
                Name = roleName
            });
            await usersService.AddToRole(_player, roleName);
        }

        var authorized = await usersService.AuthorizePolicy(_player, "SampleRole");

        authorized.Should().Be(expectedAuthorized);
        if (expectedAuthorized)
            _player.User.AuthorizedPolicies.Should().BeEquivalentTo(["SampleRole"]);
        else
            _player.User.AuthorizedPolicies.Should().BeEquivalentTo([]);
    }

    [Fact]
    public async Task PlayerInvokeShouldWork()
    {
        int invokedTimes = 0;

        async Task act() => await _player.Invoke(() =>
        {
            invokedTimes++;
            return Task.CompletedTask;
        });

        await act();
        await act();

        invokedTimes.Should().Be(2);
    }

    [Fact]
    public async Task PlayerInvokeShouldWorkWhenExceptionThrown()
    {
        int invokedTimes = 0;

        var act = async () => await _player.Invoke(() =>
        {
            invokedTimes++;
            throw new Exception();
        });

        await act.Should().ThrowAsync<Exception>();
        await act.Should().ThrowAsync<Exception>();

        invokedTimes.Should().Be(2);
    }

    [Fact]
    public async Task RecursiveInvokeShouldNotBlock()
    {
        bool invoked = false;
        await _player.Invoke(async () =>
        {
            await _player.Invoke(() =>
            {
                invoked = true;
                return Task.CompletedTask;
            });
        });

        invoked.Should().BeTrue();
    }
}
