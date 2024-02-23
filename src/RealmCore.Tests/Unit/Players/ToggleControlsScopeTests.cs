namespace RealmCore.Tests.Unit.Players;

public class ToggleControlsScopeTests : RealmUnitTestingBase
{
    [Fact]
    public void ItShouldWork()
    {
        var server = CreateServer();
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
        var player = CreateServerWithOnePlayer();

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
