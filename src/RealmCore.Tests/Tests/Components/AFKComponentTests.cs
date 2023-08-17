namespace RealmCore.Tests.Tests.Components;

public class AFKComponentTests
{
    private readonly Entity _entity;
    private readonly AFKComponent _afkComponent;
    private readonly TestDateTimeProvider _testDateTimeProvider;
    public AFKComponentTests()
    {
        var services = new ServiceCollection();
        _testDateTimeProvider = new();

        var serviceProvider = services.BuildServiceProvider();

        _entity = new(serviceProvider, "test");
        _afkComponent = _entity.AddComponent<AFKComponent>();
    }

    [Fact]
    public void DestroyingComponentShouldReset()
    {
        bool _isAfk = false;
        TimeSpan _elapsed = TimeSpan.Zero;

        _afkComponent.StateChanged += (AFKComponent _, bool isAfk, TimeSpan elapsed) =>
        {
            _isAfk = isAfk;
            _elapsed = elapsed;
        };

        _afkComponent.IsAFK.Should().BeFalse();

        _afkComponent.HandlePlayerAFKStarted(_testDateTimeProvider.Now);
        _elapsed.Should().Be(TimeSpan.Zero);
        _isAfk.Should().BeTrue();
        _afkComponent.IsAFK.Should().BeTrue();

        _testDateTimeProvider.AddOffset(TimeSpan.FromMinutes(5));
        _afkComponent.HandlePlayerAFKStopped(_testDateTimeProvider.Now);

        _elapsed.Should().Be(TimeSpan.FromMinutes(5));
        _isAfk.Should().BeFalse();
        _afkComponent.IsAFK.Should().BeFalse();
    }
}
