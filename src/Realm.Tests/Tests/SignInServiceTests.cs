using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
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
    private readonly Mock<ILogger<SignInService>> _logger;

    public SignInServiceTests()
    {
        _realmTestingServer = new();
        _entityHelper = new(_realmTestingServer);
        _logger = new Mock<ILogger<SignInService>>();

        _signInService = new SignInService(new TestItemsRegistry(), _logger.Object);
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
