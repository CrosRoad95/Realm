namespace RealmCore.Tests.Unit.Players;

public class PlayerSettingsFeatureTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>, IDisposable
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingServerHosting<RealmTestingPlayer> _hosting;
    private readonly RealmTestingPlayer _player;
    private readonly PlayerSettingsFeature _settings;

    public PlayerSettingsFeatureTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _hosting = fixture.Hosting;
        _settings = _player.Settings;
    }

    [Fact]
    public void SsettingsShouldWork()
    {
        using var monitor = _settings.Monitor();
        var had = _settings.Has(1);
        _settings.Set(1, "foo");
        _settings.Set(1, "bar");
        var has = _settings.Has(1);
        var hasValue1 = _settings.TryGet(1, out var value1);
        var hasValue2 = _settings.TryGet(2, out var value2);
        var removed1 = _settings.TryRemove(1);
        var removed2 = _settings.TryRemove(1);

        using var _ = new AssertionScope();
        had.Should().BeFalse();
        has.Should().BeTrue();
        hasValue1.Should().BeTrue();
        value1.Should().Be("bar");
        hasValue2.Should().BeFalse();
        value2.Should().BeNull();
        removed1.Should().BeTrue();
        removed2.Should().BeFalse();
        monitor.GetOccurredEvents().Should().BeEquivalentTo(["Changed", "VersionIncreased", "Changed", "VersionIncreased", "Removed", "VersionIncreased"]);
    }

    public void Dispose()
    {
        _settings.Reset();
    }
}
