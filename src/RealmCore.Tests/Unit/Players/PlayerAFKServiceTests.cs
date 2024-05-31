namespace RealmCore.Tests.Unit.Players;

public class PlayerAFKServiceTests
{
    [Fact]
    public async Task ServiceShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var afkService = player.AFK;
        var dateTimeProvider = hosting.DateTimeProvider;

        bool _isAfk = false;
        TimeSpan _elapsed = TimeSpan.Zero;

        afkService.StateChanged += (_, isAfk, elapsed) =>
        {
            _isAfk = isAfk;
            _elapsed = elapsed;
        };

        afkService.IsAFK.Should().BeFalse();

        var debounce = hosting.TestDebounceFactory.LastDebounce;
        afkService.HandleAFKStarted();
        await debounce.Release();
        _elapsed.Should().Be(TimeSpan.Zero);
        _isAfk.Should().BeTrue();
        afkService.IsAFK.Should().BeTrue();

        dateTimeProvider.AddOffset(TimeSpan.FromMinutes(5));
        afkService.HandleAFKStopped();

        _elapsed.Should().Be(TimeSpan.FromMinutes(5));
        _isAfk.Should().BeFalse();
        afkService.IsAFK.Should().BeFalse();
    }
}
