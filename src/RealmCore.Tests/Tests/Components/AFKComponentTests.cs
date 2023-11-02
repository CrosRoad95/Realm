namespace RealmCore.Tests.Tests.Components;

public class AFKComponentTests
{
    [Fact]
    public void DestroyingComponentShouldReset()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var afkComponent = player.AddComponent<AFKComponent>();

        bool _isAfk = false;
        TimeSpan _elapsed = TimeSpan.Zero;

        afkComponent.StateChanged += (AFKComponent _, bool isAfk, TimeSpan elapsed) =>
        {
            _isAfk = isAfk;
            _elapsed = elapsed;
        };

        afkComponent.IsAFK.Should().BeFalse();

        afkComponent.HandlePlayerAFKStarted(realmTestingServer.TestDateTimeProvider.Now);
        _elapsed.Should().Be(TimeSpan.Zero);
        _isAfk.Should().BeTrue();
        afkComponent.IsAFK.Should().BeTrue();

        realmTestingServer.TestDateTimeProvider.AddOffset(TimeSpan.FromMinutes(5));
        afkComponent.HandlePlayerAFKStopped(realmTestingServer.TestDateTimeProvider.Now);

        _elapsed.Should().Be(TimeSpan.FromMinutes(5));
        _isAfk.Should().BeFalse();
        afkComponent.IsAFK.Should().BeFalse();
    }
}
