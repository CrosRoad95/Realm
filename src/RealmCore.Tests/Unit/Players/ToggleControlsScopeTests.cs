namespace RealmCore.Tests.Unit.Players;

public class ToggleControlsScopeTests : RealmUnitTestingBase
{
    [Fact]
    public void ItShouldWork()
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();

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
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();

        var createScope = () => new ToggleControlsScope(player);
        if (dispose)
        {
            createScope().Dispose();
        }
        createScope.Should().NotThrow();
        createScope.Should().Throw<InvalidOperationException>();
    }
}
