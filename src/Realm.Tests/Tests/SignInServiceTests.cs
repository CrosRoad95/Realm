using Microsoft.AspNetCore.Identity;
using Realm.Persistance.Data;

namespace Realm.Tests.Tests;

public class SignInServiceTests
{
    private readonly EntityHelper _entityHelper;
    private readonly RealmTestingServer _realmTestingServer;
    private readonly IUsersService _signInService;

    public SignInServiceTests()
    {
        _realmTestingServer = new(new(), new(5000));
        _entityHelper = new(_realmTestingServer);

        _signInService = _realmTestingServer.GetRequiredService<IUsersService>();
    }

    [Fact]
    public async Task SignInShouldAddAllNeededComponents()
    {
        #region Arrange
        var user = new User
        {
            UserName = Guid.NewGuid().ToString()[..8],
        };
        
        await _realmTestingServer.GetRequiredService<UserManager<User>>().CreateAsync(user, "asdASD123!@#");

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
