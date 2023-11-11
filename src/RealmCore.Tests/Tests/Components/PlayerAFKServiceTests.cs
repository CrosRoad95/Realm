namespace RealmCore.Tests.Tests.Components;

public class PlayerAFKServiceTests
{
    [Fact]
    public async Task ServiceShouldWork()
    {
        var realmTestingServer = new RealmTestingServer(new TestConfigurationProvider());
        var player = realmTestingServer.CreatePlayer();
        var afkService = player.GetRequiredService<IPlayerAFKService>();
        var dateTimeProvider = realmTestingServer.TestDateTimeProvider;

        bool _isAfk = false;
        TimeSpan _elapsed = TimeSpan.Zero;

        afkService.StateChanged += (IPlayerAFKService _, bool isAfk, TimeSpan elapsed) =>
        {
            _isAfk = isAfk;
            _elapsed = elapsed;
        };

        afkService.IsAFK.Should().BeFalse();

        var debounce = realmTestingServer.TestDebounceFactory.LastDebounce;
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
