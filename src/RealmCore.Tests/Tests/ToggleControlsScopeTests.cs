using RealmCore.Server.Scopes;

namespace RealmCore.Tests.Tests;

public class ToggleControlsScopeTests
{
    [Fact]
    public void ItShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        var scope = new ToggleControlsScope(player);
        player.Controls.JumpEnabled = false;
        scope.Dispose();
        player.Controls.JumpEnabled.Should().BeTrue();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void OnlyOneScopePerPlayerIsAllowed1(bool dispose)
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        var createScope = () => new ToggleControlsScope(player);
        if (dispose)
        {
            createScope().Dispose();
        }
        createScope.Should().NotThrow();
        createScope.Should().Throw<InvalidOperationException>();
    }
}
