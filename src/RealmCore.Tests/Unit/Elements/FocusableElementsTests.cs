namespace RealmCore.Tests.Unit.Elements;

public class FocusableElementsTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingServerHosting<RealmTestingPlayer> _hosting;
    private readonly RealmTestingPlayer _player;

    public FocusableElementsTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _hosting = fixture.Hosting;
    }

    [Fact]
    public void FocusableShouldWork()
    {
        var obj = _hosting.CreateFocusableObject();
        obj.AddFocusedPlayer(_player);

        obj.FocusedPlayerCount.Should().Be(1);
        obj.FocusedPlayers.First().Should().BeEquivalentTo(_player);
    }

    [Fact]
    public async Task FocusedElementShouldBeRemovedWhenItDisposes()
    {
        var player = await _hosting.CreatePlayer();
        var obj = _hosting.CreateFocusableObject();
        obj.AddFocusedPlayer(player);
        
        await _hosting.DisconnectPlayer(player);

        obj.FocusedPlayerCount.Should().Be(0);
    }

    [Fact]
    public void YouCanNotFocusOneElementTwoTimes()
    {
        var obj = _hosting.CreateFocusableObject();

        var act = () => obj.AddFocusedPlayer(_player);

        act().Should().BeTrue();
        act().Should().BeFalse();
        obj.FocusedPlayerCount.Should().Be(1);
    }

    [Fact]
    public void YouShouldBeAbleToRemoveFocusedPlayer()
    {
        var obj = _hosting.CreateFocusableObject();

        obj.AddFocusedPlayer(_player).Should().BeTrue();
        obj.RemoveFocusedPlayer(_player).Should().BeTrue();
        obj.RemoveFocusedPlayer(_player).Should().BeFalse();
        obj.FocusedPlayerCount.Should().Be(0);
    }

    [Fact]
    public void RemovedFocusedElementShouldWork()
    {
        var obj = _hosting.CreateFocusableObject();
        obj.AddFocusedPlayer(_player);

        bool lostFocus = false;
        obj.PlayerLostFocus += (s, plr) =>
        {
            if (plr == _player)
                lostFocus = true;
        };

        obj.AddFocusedPlayer(_player);
        _player.Destroy();
        lostFocus.Should().BeTrue();
    }
}
