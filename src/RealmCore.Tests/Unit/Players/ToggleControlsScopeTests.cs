namespace RealmCore.Tests.Unit.Players;

public class ToggleControlsScopeTests
{
    [Fact]
    public async Task ItShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var scope = new ToggleControlsScope(player);
        player.Controls.JumpEnabled = false;
        scope.Dispose();
        player.Controls.JumpEnabled.Should().BeTrue();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task OnlyOneScopePerPlayerIsAllowed1(bool dispose)
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        ToggleControlsScope? createdScope = null;

        var createScope = () =>
        {
            createdScope = new ToggleControlsScope(player);
            return createdScope;
        };

        createScope.Should().NotThrow();
        if (dispose)
        {
            createdScope?.Dispose();
        }

        if(dispose)
            createScope.Should().NotThrow();
        else
            createScope.Should().Throw<InvalidOperationException>();
    }
}
