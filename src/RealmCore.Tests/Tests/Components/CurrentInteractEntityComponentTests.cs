using RealmCore.ECS;

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
        _logger.SetupLogger();
        services.AddSingleton(_logger.Object);

        var serviceProvider = services.BuildServiceProvider();

        _entity = new("test1");
        _testEntity = new("test2");
        _entity2 = new("test3");
        _testEntity2 = new("test4");
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
