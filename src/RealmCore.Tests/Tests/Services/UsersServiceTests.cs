using Microsoft.AspNetCore.Identity;
using RealmCore.Persistence.Data;

namespace RealmCore.Tests.Tests.Services;

public class UsersServiceTests
{
    private readonly EntityHelper _entityHelper;
    private readonly RealmTestingServer _realmTestingServer;
    private readonly IUsersService _signInService;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<UserData> _userManager;

    public UsersServiceTests()
    {
        _realmTestingServer = new();
        _entityHelper = new(_realmTestingServer);

        _signInService = _realmTestingServer.GetRequiredService<IUsersService>();
        _userRepository = _realmTestingServer.GetRequiredService<IUserRepository>();
        _userManager = _realmTestingServer.GetRequiredService<UserManager<UserData>>();
    }

    [Fact]
    public async Task TestSignInFlow()
    {
        #region Arrange
        var login = Guid.NewGuid().ToString()[..8];
        var password = "asdASD123!@#";
        var playerEntity = _entityHelper.CreatePlayerEntity();
        #endregion

        #region Act
        var userId = await _signInService.SignUp(login, password);
        var user = await _userRepository.GetUserByLogin(login) ?? throw new Exception("User not found");

        var validPassword = await _userManager.CheckPasswordAsync(user, password);
        var signedIn = await _signInService.SignIn(playerEntity, user);
        var lastNick = await _userRepository.GetLastNickName(userId);
        #endregion

        #region Assert
        validPassword.Should().BeTrue();
        signedIn.Should().BeTrue();
        lastNick.Should().Be("CrosRoad95");
        #endregion
    }
}
