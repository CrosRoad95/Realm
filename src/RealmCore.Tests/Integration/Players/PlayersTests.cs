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
}
