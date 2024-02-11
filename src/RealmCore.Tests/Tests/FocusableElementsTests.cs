namespace RealmCore.Tests.Tests;

public class FocusableElementsTests
{
    [Fact]
    public void FocusableShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var obj = realmTestingServer.CreateFocusableObject();
        obj.AddFocusedPlayer(player);

        obj.FocusedPlayerCount.Should().Be(1);
        obj.FocusedPlayers.Should().BeEquivalentTo(player);
    }

    [Fact]
    public void FocusedElementShouldBeRemovedWhenItDisposes()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var obj = realmTestingServer.CreateFocusableObject();

        obj.AddFocusedPlayer(player);
        player.Destroy();

        obj.FocusedPlayerCount.Should().Be(0);
    }

    [Fact]
    public void YouCanNotFocusOneElementTwoTimes()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var obj = realmTestingServer.CreateFocusableObject();
        var act = () => obj.AddFocusedPlayer(player);

        act().Should().BeTrue();
        act().Should().BeFalse();
        obj.FocusedPlayerCount.Should().Be(1);
    }

    [Fact]
    public void YouShouldBeAbleToRemoveFocusedPlayer()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var obj = realmTestingServer.CreateFocusableObject();

        obj.AddFocusedPlayer(player).Should().BeTrue();
        obj.RemoveFocusedPlayer(player).Should().BeTrue();
        obj.RemoveFocusedPlayer(player).Should().BeFalse();
        obj.FocusedPlayerCount.Should().Be(0);
    }

    [Fact]
    public void RemovedFocusedElementShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var obj = realmTestingServer.CreateFocusableObject();

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
