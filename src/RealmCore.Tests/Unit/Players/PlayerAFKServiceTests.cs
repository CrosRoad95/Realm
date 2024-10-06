namespace RealmCore.Tests.Unit.Players;

public class PlayerAFKServiceTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingPlayer _player;
    private readonly TestDateTimeProvider _dateTimeProvider;
    private readonly TestDebounceFactory _debounceFactory;

    public PlayerAFKServiceTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _dateTimeProvider = _fixture.Hosting.DateTimeProvider;
        _debounceFactory = _fixture.Hosting.DebounceFactory;
    }

    [Fact]
    public async Task ServiceShouldWork()
    {
        var afkService = _player.AFK;

        bool _isAfk = false;
        TimeSpan _elapsed = TimeSpan.Zero;

        afkService.StateChanged += (_, isAfk, elapsed) =>
        {
            _isAfk = isAfk;
            _elapsed = elapsed;
        };

        afkService.IsAFK.Should().BeFalse();

        var debounce = _debounceFactory.LastDebounce;
        afkService.HandleAFKStarted();
        await debounce!.Release();
        _elapsed.Should().Be(TimeSpan.Zero);
        _isAfk.Should().BeTrue();
        afkService.IsAFK.Should().BeTrue();

        _dateTimeProvider.Add(TimeSpan.FromMinutes(5));
        afkService.HandleAFKStopped();

        _elapsed.Should().Be(TimeSpan.FromMinutes(5));
        _isAfk.Should().BeFalse();
        afkService.IsAFK.Should().BeFalse();
    }
}
