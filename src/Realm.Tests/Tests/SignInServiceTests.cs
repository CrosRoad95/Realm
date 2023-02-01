using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Realm.Logging;
using Realm.Persistance.Data;
using Realm.Server.Interfaces;
using Realm.Server.Services;
using Realm.Tests.Helpers;

namespace Realm.Tests.Tests;

public class SignInServiceTests
{
    private readonly EntityHelper _entityHelper;
    private readonly RealmTestingServer _realmTestingServer;
    private readonly ISignInService _signInService;
    public SignInServiceTests()
    {
        _realmTestingServer = new();
        _entityHelper = new(_realmTestingServer);
        _signInService = new SignInService(new TestItemsRegistry(), new RealmLogger().GetLogger());
    }

    //[Fact]
    public async Task SignInShouldAddAllNeededComponents()
    {
        var user = new User
        {
            UserName = Guid.NewGuid().ToString()[..8],
        };
        
        await _realmTestingServer.GetRequiredService<UserManager<User>>().CreateAsync(user, "asdASD123!@#");

        var playerEntity = _entityHelper.CreatePlayerEntity();
        var signedIn = await _signInService.SignIn(playerEntity, user);

        signedIn.Should().BeTrue();
    }
}
