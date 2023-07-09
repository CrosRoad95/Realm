using Microsoft.AspNetCore.Identity;
using RealmCore.Persistance.Data;

namespace RealmCore.Tests.Tests;

public class SignInServiceTests
{
    private readonly EntityHelper _entityHelper;
    private readonly RealmTestingServer _realmTestingServer;
    private readonly IUsersService _signInService;

    public SignInServiceTests()
    {
        _realmTestingServer = new();
        _entityHelper = new(_realmTestingServer);

        _signInService = _realmTestingServer.GetRequiredService<IUsersService>();
    }

    //[Fact]
    public async Task SignInShouldAddAllNeededComponents()
    {
        #region Arrange
        var user = new UserData
        {
            UserName = Guid.NewGuid().ToString()[..8],
        };
        
        await _realmTestingServer.GetRequiredService<UserManager<UserData>>().CreateAsync(user, "asdASD123!@#");

        var playerEntity = _entityHelper.CreatePlayerEntity();
        #endregion

        #region Act
        var signedIn = await _signInService.SignIn(playerEntity, user);
        #endregion

        #region Assert
        signedIn.Should().BeTrue();
        #endregion
    }
}
