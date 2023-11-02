namespace RealmCore.Tests.Tests.Components;

public class FocusableComponentTests
{
    [Fact]
    public void FocusableComponentShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player1 = realmTestingServer.CreatePlayer();
        var player2 = realmTestingServer.CreatePlayer();
        var focusableComponent = player1.AddComponent<FocusableComponent>();
        focusableComponent.AddFocusedPlayer(player2);

        focusableComponent.FocusedPlayerCount.Should().Be(1);
        focusableComponent.FocusedPlayers.Should().BeEquivalentTo(player2);
    }

    [Fact]
    public void YouShouldNotBeAbleToFocusItself()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        var focusableComponent = player.AddComponent<FocusableComponent>();
        var act = () => focusableComponent.AddFocusedPlayer(player);

        act.Should().Throw<InvalidOperationException>();
        focusableComponent.FocusedPlayerCount.Should().Be(0);
    }

    [Fact]
    public void FocusedElementShouldBeRemovedWhenItDisposes()
    {
        var realmTestingServer = new RealmTestingServer();
        var player1 = realmTestingServer.CreatePlayer();
        var player2 = realmTestingServer.CreatePlayer();
        var focusableComponent = player1.AddComponent<FocusableComponent>();

        focusableComponent.AddFocusedPlayer(player2);
        player2.Destroy();

        focusableComponent.FocusedPlayerCount.Should().Be(0);
    }
    
    [Fact]
    public void YouCanNotFocusOneElementTwoTimes()
    {
        var realmTestingServer = new RealmTestingServer();
        var player1 = realmTestingServer.CreatePlayer();
        var player2 = realmTestingServer.CreatePlayer();
        var focusableComponent = player1.AddComponent<FocusableComponent>();
        var act = () => focusableComponent.AddFocusedPlayer(player2);

        act().Should().BeTrue();
        act().Should().BeFalse();
        focusableComponent.FocusedPlayerCount.Should().Be(1);
    }
    
    [Fact]
    public void YouShouldBeAbleToRemoveFocusedPlayer()
    {
        var realmTestingServer = new RealmTestingServer();
        var player1 = realmTestingServer.CreatePlayer();
        var player2 = realmTestingServer.CreatePlayer();
        var focusableComponent = player1.AddComponent<FocusableComponent>();

        focusableComponent.AddFocusedPlayer(player2).Should().BeTrue();
        focusableComponent.RemoveFocusedPlayer(player2).Should().BeTrue();
        focusableComponent.RemoveFocusedPlayer(player2).Should().BeFalse();
        focusableComponent.FocusedPlayerCount.Should().Be(0);
    }
    
    
    [Fact]
    public void RemovedFocusedComponentShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player1 = realmTestingServer.CreatePlayer();
        var player2 = realmTestingServer.CreatePlayer();
        var focusableComponent = player1.AddComponent<FocusableComponent>();

        bool lostFocus = false;
        focusableComponent.PlayerLostFocus += (s, e) =>
        {
            if (e == player2)
                lostFocus = true;
        };

        focusableComponent.AddFocusedPlayer(player2);
        player1.DestroyComponent(focusableComponent);
        lostFocus.Should().BeTrue();
    }

}
