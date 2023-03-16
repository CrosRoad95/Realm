using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Realm.Persistance.Data;
using Realm.Server.Interfaces;
using Realm.Server.Services;

namespace Realm.Tests.Tests;

public class SignInServiceTests
{
    private readonly EntityHelper _entityHelper;
    private readonly RealmTestingServer _realmTestingServer;
    private readonly IRPGUserManager _signInService;

    public SignInServiceTests()
    {
        _realmTestingServer = new(new(), new(5000));
        _entityHelper = new(_realmTestingServer);

        _signInService = _realmTestingServer.GetRequiredService<IRPGUserManager>();
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
