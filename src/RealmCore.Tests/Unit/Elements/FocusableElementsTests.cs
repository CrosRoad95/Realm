namespace RealmCore.Tests.Unit.Elements;

public class FocusableElementsTests
{
    [Fact]
    public async Task FocusableShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var obj = hosting.CreateFocusableObject();
        obj.AddFocusedPlayer(player);

        obj.FocusedPlayerCount.Should().Be(1);
        obj.FocusedPlayers.First().Should().BeEquivalentTo(player);
    }

    [Fact]
    public async Task FocusedElementShouldBeRemovedWhenItDisposes()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var obj = hosting.CreateFocusableObject();

        obj.AddFocusedPlayer(player);
        player.Destroy();

        obj.FocusedPlayerCount.Should().Be(0);
    }

    [Fact]
    public async Task YouCanNotFocusOneElementTwoTimes()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var obj = hosting.CreateFocusableObject();
        var act = () => obj.AddFocusedPlayer(player);

        act().Should().BeTrue();
        act().Should().BeFalse();
        obj.FocusedPlayerCount.Should().Be(1);
    }

    [Fact]
    public async Task YouShouldBeAbleToRemoveFocusedPlayer()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var obj = hosting.CreateFocusableObject();

        obj.AddFocusedPlayer(player).Should().BeTrue();
        obj.RemoveFocusedPlayer(player).Should().BeTrue();
        obj.RemoveFocusedPlayer(player).Should().BeFalse();
        obj.FocusedPlayerCount.Should().Be(0);
    }

    [Fact]
    public async Task RemovedFocusedElementShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var obj = hosting.CreateFocusableObject();

        bool lostFocus = false;
        obj.PlayerLostFocus += (s, plr) =>
        {
            if (plr == player)
                lostFocus = true;
        };

        obj.AddFocusedPlayer(player);
        player.Destroy();
        lostFocus.Should().BeTrue();
    }

}
