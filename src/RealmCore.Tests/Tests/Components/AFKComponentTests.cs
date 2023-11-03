namespace RealmCore.Tests.Tests.Components;

public class AFKComponentTests
{
    [Fact]
    public async Task DestroyingComponentShouldReset()
    {
        var realmTestingServer = new RealmTestingServer(new TestConfigurationProvider());
        var player = realmTestingServer.CreatePlayer();
        var afkComponent = player.AddComponentWithDI<AFKComponent>();
        var dateTimeProvider = realmTestingServer.TestDateTimeProvider;

        bool _isAfk = false;
        TimeSpan _elapsed = TimeSpan.Zero;

        afkComponent.StateChanged += (AFKComponent _, bool isAfk, TimeSpan elapsed) =>
        {
            _isAfk = isAfk;
            _elapsed = elapsed;
        };

        afkComponent.IsAFK.Should().BeFalse();

        afkComponent.HandlePlayerAFKStarted(dateTimeProvider.Now);
        await Task.Delay(200);
        _elapsed.Should().Be(TimeSpan.Zero);
        _isAfk.Should().BeTrue();
        afkComponent.IsAFK.Should().BeTrue();

        dateTimeProvider.AddOffset(TimeSpan.FromMinutes(5));
        afkComponent.HandlePlayerAFKStopped(dateTimeProvider.Now);

        _elapsed.Should().Be(TimeSpan.FromMinutes(5));
        _isAfk.Should().BeFalse();
        afkComponent.IsAFK.Should().BeFalse();
    }
}
