namespace RealmCore.Tests.Integration.Players;

public class PlayersTests : RealmIntegrationTestingBase
{
    protected override string DatabaseName => "PlayersTests";

    [InlineData(new string[] { }, false)]
    [InlineData(new string[] { "Admin" }, true)]
    [Theory]
    public async Task UserShouldAuthorize(string[] roles, bool expectedAuthorized)
    {
        #region Arrange
        var server = await CreateServerAsync();
        var player = await CreatePlayerAsync();

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
        var authorized = await usersService.AuthorizePolicy(player, "Admin");
        #endregion

        #region Assert
        authorized.Should().Be(expectedAuthorized);
        if (expectedAuthorized)
            player.User.AuthorizedPolicies.Should().BeEquivalentTo(["Admin"]);
        else
            player.User.AuthorizedPolicies.Should().BeEquivalentTo([]);
        #endregion
    }
}
