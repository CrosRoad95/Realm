namespace RealmCore.Tests.Tests.Components;

public class CurrentInteractEntityComponentTests
{
    private readonly Entity _entity;
    private readonly Entity _entity2;
    private readonly Entity _testEntity;
    private readonly Entity _testEntity2;
    private readonly Mock<ILogger<Entity>> _logger = new(MockBehavior.Strict);

    public CurrentInteractEntityComponentTests()
    {
        var services = new ServiceCollection();
        _logger.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
            .Verifiable();
        services.AddSingleton(_logger.Object);
        _logger.Setup(x => x.BeginScope(It.IsAny<Dictionary<string, object>>())).Returns((IDisposable)null);

        var serviceProvider = services.BuildServiceProvider();

        _entity = new(serviceProvider, "test1", EntityTag.Unknown);
        _testEntity = new(serviceProvider, "test2", EntityTag.Unknown);
        _entity2 = new(serviceProvider, "test3", EntityTag.Unknown);
        _testEntity2 = new(serviceProvider, "test4", EntityTag.Unknown);
    }

    [Fact]
    public void DestroyingComponentShouldReset()
    {
        #region Arrange
        var currentInteractionComponent = _entity.AddComponent(new CurrentInteractEntityComponent(_testEntity));
        currentInteractionComponent.CurrentInteractEntity.Should().Be(_testEntity);
        #endregion

        #region Act
        _entity.DestroyComponent(currentInteractionComponent);
        #endregion

        #region Assert
        currentInteractionComponent.CurrentInteractEntity.Should().BeNull();
        #endregion
    }

    [Fact]
    public void DestroyingEntityShouldResetAndRemoveComponent()
    {
        #region Arrange
        var currentInteractionComponent = _entity2.AddComponent(new CurrentInteractEntityComponent(_testEntity2));
        currentInteractionComponent.CurrentInteractEntity.Should().Be(_testEntity2);
        #endregion

        #region Act
        _testEntity2.Dispose();
        #endregion

        #region Assert
        currentInteractionComponent.CurrentInteractEntity.Should().BeNull();
        _entity2.Components.Should().BeEmpty();
        #endregion
    }
}
