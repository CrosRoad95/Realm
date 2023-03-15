using Realm.Domain.Components.Players;
using Realm.Domain;
using FluentAssertions;
using Realm.Common.Providers;
using Realm.Tests.Providers;

namespace Realm.Tests.Tests.Components;

public class AFKComponentTests
{
    private readonly Entity _entity;
    private readonly AFKComponent _afkComponent;
    private readonly TestDateTimeProvider _testDateTimeProvider;
    public AFKComponentTests()
    {
        var services = new ServiceCollection();
        _testDateTimeProvider = new();
        services.AddSingleton<IDateTimeProvider>(_testDateTimeProvider);

        var serviceProvider = services.BuildServiceProvider();

        _entity = new(serviceProvider, "test", Entity.EntityTag.Unknown);
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

        _afkComponent.HandlePlayerAFKStarted();
        _elapsed.Should().Be(TimeSpan.Zero);
        _isAfk.Should().BeTrue();
        _afkComponent.IsAFK.Should().BeTrue();

        _testDateTimeProvider.AddOffset(TimeSpan.FromMinutes(5));
        _afkComponent.HandlePlayerAFKStopped();

        _elapsed.Should().Be(TimeSpan.FromMinutes(5));
        _isAfk.Should().BeFalse();
        _afkComponent.IsAFK.Should().BeFalse();
    }
}
