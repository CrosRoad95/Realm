using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Realm.Domain.Options;
using Realm.Persistance.Data;
using Realm.Server.Interfaces;
using Realm.Server.Services;
using Realm.Tests.Helpers;
using Realm.Tests.Providers;

namespace Realm.Tests.Tests;

public class SignInServiceTests
{
    private readonly EntityHelper _entityHelper;
    private readonly RealmTestingServer _realmTestingServer;
    private readonly TestDateTimeProvider _testDateTimeProvider;
    private readonly IRPGUserManager _signInService;
    private readonly Mock<ILogger<RPGUserManager>> _logger;

    public SignInServiceTests()
    {
        _realmTestingServer = new();
        _testDateTimeProvider = new();
        _entityHelper = new(_realmTestingServer);
        _logger = new Mock<ILogger<RPGUserManager>>();

        _signInService = new RPGUserManager(new TestItemsRegistry(), _realmTestingServer.GetRequiredService<UserManager<User>>(), _logger.Object,
            _realmTestingServer.GetRequiredService<IOptions<GameplayOptions>>(), _testDateTimeProvider);
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
