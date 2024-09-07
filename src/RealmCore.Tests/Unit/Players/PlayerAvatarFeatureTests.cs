namespace RealmCore.Tests.Unit.Players;

public class PlayerAvatarFeatureTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingPlayer _player;

    public PlayerAvatarFeatureTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
    }

    [Fact]
    public void AvatarsShouldWork()
    {
        var avatar = _player.Avatar;
        using var monitor = avatar.Monitor();

        var current = avatar.Current;
        avatar.Current = null;
        avatar.Current = "foo";

        using var _ = new AssertionScope();
        current.Should().BeNull();
        avatar.Current.Should().BeNull();
        monitor.GetOccurredEvents().Should().BeEquivalentTo(["Changed", "VersionIncreased"]);
    }
}
