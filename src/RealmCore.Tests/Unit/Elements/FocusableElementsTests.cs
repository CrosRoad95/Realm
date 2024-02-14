namespace RealmCore.Tests.Unit.Elements;

public class FocusableElementsTests : RealmUnitTestingBase
{
    [Fact]
    public void FocusableShouldWork()
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();
        var obj = realmTestingServer.CreateFocusableObject();
        obj.AddFocusedPlayer(player);

        obj.FocusedPlayerCount.Should().Be(1);
        obj.FocusedPlayers.Should().BeEquivalentTo(player);
    }

    [Fact]
    public void FocusedElementShouldBeRemovedWhenItDisposes()
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();
        var obj = realmTestingServer.CreateFocusableObject();

        obj.AddFocusedPlayer(player);
        player.Destroy();

        obj.FocusedPlayerCount.Should().Be(0);
    }

    [Fact]
    public void YouCanNotFocusOneElementTwoTimes()
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();
        var obj = realmTestingServer.CreateFocusableObject();
        var act = () => obj.AddFocusedPlayer(player);

        act().Should().BeTrue();
        act().Should().BeFalse();
        obj.FocusedPlayerCount.Should().Be(1);
    }

    [Fact]
    public void YouShouldBeAbleToRemoveFocusedPlayer()
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();
        var obj = realmTestingServer.CreateFocusableObject();

        obj.AddFocusedPlayer(player).Should().BeTrue();
        obj.RemoveFocusedPlayer(player).Should().BeTrue();
        obj.RemoveFocusedPlayer(player).Should().BeFalse();
        obj.FocusedPlayerCount.Should().Be(0);
    }

    [Fact]
    public void RemovedFocusedElementShouldWork()
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();
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
