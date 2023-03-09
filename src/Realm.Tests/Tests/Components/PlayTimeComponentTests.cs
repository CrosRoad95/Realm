using Realm.Domain.Components.Players;
using Realm.Domain.Options;
using Realm.Domain;
using Realm.Common.Providers;
using Realm.Tests.Providers;
using FluentAssertions;

namespace Realm.Tests.Tests.Components;

public class PlayTimeComponentTests
{
    private readonly Entity _entity1;
    private readonly Entity _entity2;
    private readonly PlayTimeComponent _playTimeComponent;
    private readonly PlayTimeComponent _playTimeComponentWithInitialState;
    private readonly TestDateTimeProvider _testDateTimeProvider;

    public PlayTimeComponentTests()
    {
        _testDateTimeProvider = new();
        var services = new ServiceCollection();
        services.AddSingleton<IDateTimeProvider>(_testDateTimeProvider);

        var serviceProvider = services.BuildServiceProvider();
        _entity1 = new(serviceProvider, "test", Entity.EntityTag.Unknown);
        _entity2 = new(serviceProvider, "test with initial state", Entity.EntityTag.Unknown);
        _playTimeComponent = new();
        _playTimeComponentWithInitialState = new(1000);
        _entity1.AddComponent(_playTimeComponent);
        _entity2.AddComponent(_playTimeComponentWithInitialState);
    }

    [Fact]
    public void TestIfCounterWorksCorrectly()
    {
        _playTimeComponent.PlayTime.Should().Be(0);
        _playTimeComponent.TotalPlayTime.Should().Be(0);

        _playTimeComponentWithInitialState.PlayTime.Should().Be(0);
        _playTimeComponentWithInitialState.TotalPlayTime.Should().Be(1000);

        _testDateTimeProvider.AddOffset(TimeSpan.FromSeconds(50));

        _playTimeComponent.PlayTime.Should().Be(50);
        _playTimeComponent.TotalPlayTime.Should().Be(50);

        _playTimeComponentWithInitialState.PlayTime.Should().Be(50);
        _playTimeComponentWithInitialState.TotalPlayTime.Should().Be(1050);
    }
}
