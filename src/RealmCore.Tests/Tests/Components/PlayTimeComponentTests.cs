namespace RealmCore.Tests.Tests.Components;

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
        _entity1 = new(serviceProvider, "test", EntityTag.Unknown);
        _entity2 = new(serviceProvider, "test with initial state", EntityTag.Unknown);
        _playTimeComponent = new();
        _playTimeComponentWithInitialState = new(1000);
        _entity1.AddComponent(_playTimeComponent);
        _entity2.AddComponent(_playTimeComponentWithInitialState);
    }

    [Fact]
    public void TestIfCounterWorksCorrectly()
    {
        _playTimeComponent.PlayTime.Should().Be(TimeSpan.Zero);
        _playTimeComponent.TotalPlayTime.Should().Be(TimeSpan.Zero);

        _playTimeComponentWithInitialState.PlayTime.Should().Be(TimeSpan.Zero);
        _playTimeComponentWithInitialState.TotalPlayTime.Should().Be(TimeSpan.FromSeconds(1000));

        _testDateTimeProvider.AddOffset(TimeSpan.FromSeconds(50));

        _playTimeComponent.PlayTime.Should().Be(TimeSpan.FromSeconds(50));
        _playTimeComponent.TotalPlayTime.Should().Be(TimeSpan.FromSeconds(50));

        _playTimeComponentWithInitialState.PlayTime.Should().Be(TimeSpan.FromSeconds(50));
        _playTimeComponentWithInitialState.TotalPlayTime.Should().Be(TimeSpan.FromSeconds(1050));
    }
}
