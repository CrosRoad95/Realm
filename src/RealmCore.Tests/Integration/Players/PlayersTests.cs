namespace RealmCore.Tests.Integration.Players;

public class PlayersTests
{
    //[InlineData(new string[] { }, false)]
    //[InlineData(new string[] { "SampleRole" }, true)]
    //[Theory]
    public async Task UserShouldAuthorize(string[] roles, bool expectedAuthorized)
    {
        #region Arrange
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var usersService = player.GetRequiredService<IUsersService>();
        foreach (var roleName in roles)
        {
            var roleManager = player.GetRequiredService<RoleManager<RoleData>>();
            await roleManager.CreateAsync(new RoleData
            {
                Name = roleName
            });
            await usersService.AddToRole(player, roleName);
        }
        #endregion

        #region Act
        var authorized = await usersService.AuthorizePolicy(player, "SampleRole");
        #endregion

        #region Assert
        authorized.Should().Be(expectedAuthorized);
        if (expectedAuthorized)
            player.User.AuthorizedPolicies.Should().BeEquivalentTo(["SampleRole"]);
        else
            player.User.AuthorizedPolicies.Should().BeEquivalentTo([]);
        #endregion
    }

    [Fact]
    public async Task PlayerInvokeShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        int invokedTimes = 0;

        async Task act() => await player.Invoke(() =>
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
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        int invokedTimes = 0;

        var act = async () => await player.Invoke(() =>
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
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        bool invoked = false;
        await player.Invoke(async () =>
        {
            await player.Invoke(() =>
            {
                invoked = true;
                return Task.CompletedTask;
            });
        });

        invoked.Should().BeTrue();
    }
}
