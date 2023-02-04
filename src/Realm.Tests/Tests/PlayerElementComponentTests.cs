using Realm.Tests.Helpers;
using Realm.Tests.TestServers;

namespace Realm.Tests.Tests;

public class PlayerElementComponentTests
{
    private readonly RealmTestingServer _server;
    private readonly EntityHelper _entityHelper;

    public PlayerElementComponentTests()
    {
        _server = new();
        _entityHelper = new(_server);
    }

    //[Fact]
    public async Task TestBinds()
    {
        var playerEntity = _entityHelper.CreatePlayerEntity();
        ;
    }
}
