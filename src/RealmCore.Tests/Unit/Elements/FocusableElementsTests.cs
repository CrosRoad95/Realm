namespace RealmCore.Tests.Unit.Elements;

public class FocusableElementsTests : RealmUnitTestingBase
{
    [Fact]
    public void FocusableShouldWork()
    {
        var server = CreateServer();
        var player = CreatePlayer();
        var obj = server.CreateFocusableObject();
        obj.AddFocusedPlayer(player);

        obj.FocusedPlayerCount.Should().Be(1);
        obj.FocusedPlayers.First().Should().BeEquivalentTo(player);
    }

    [Fact]
    public void FocusedElementShouldBeRemovedWhenItDisposes()
    {
        var server = CreateServer();
        var player = CreatePlayer();
        var obj = server.CreateFocusableObject();

        obj.AddFocusedPlayer(player);
        player.Destroy();

        obj.FocusedPlayerCount.Should().Be(0);
    }

    [Fact]
    public void YouCanNotFocusOneElementTwoTimes()
    {
        var server = CreateServer();
        var player = CreatePlayer();
        var obj = server.CreateFocusableObject();
        var act = () => obj.AddFocusedPlayer(player);

        act().Should().BeTrue();
        act().Should().BeFalse();
        obj.FocusedPlayerCount.Should().Be(1);
    }

    [Fact]
    public void YouShouldBeAbleToRemoveFocusedPlayer()
    {
        var server = CreateServer();
        var player = CreatePlayer();
        var obj = server.CreateFocusableObject();

        obj.AddFocusedPlayer(player).Should().BeTrue();
        obj.RemoveFocusedPlayer(player).Should().BeTrue();
        obj.RemoveFocusedPlayer(player).Should().BeFalse();
        obj.FocusedPlayerCount.Should().Be(0);
    }

    [Fact]
    public void RemovedFocusedElementShouldWork()
    {
        var server = CreateServer();
        var player = CreatePlayer();
        var obj = server.CreateFocusableObject();

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
