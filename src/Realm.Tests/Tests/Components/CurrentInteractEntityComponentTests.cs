using Realm.Domain.Components.Players;
using Realm.Domain;
using FluentAssertions;

namespace Realm.Tests.Tests.Components;

public class CurrentInteractEntityComponentTests
{
    private readonly Entity _entity;
    private readonly Entity _entity2;
    private readonly Entity _testEntity;
    private readonly Entity _testEntity2;

    public CurrentInteractEntityComponentTests()
    {
        var services = new ServiceCollection();

        var serviceProvider = services.BuildServiceProvider();

        _entity = new(serviceProvider, "test1", Entity.EntityTag.Unknown);
        _testEntity = new(serviceProvider, "test2", Entity.EntityTag.Unknown);
        _entity2 = new(serviceProvider, "test3", Entity.EntityTag.Unknown);
        _testEntity2 = new(serviceProvider, "test4", Entity.EntityTag.Unknown);
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
