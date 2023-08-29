using RealmCore.ECS;
using RealmCore.Server.Components.Object;
using RealmCore.Tests.Classes.Components;

namespace RealmCore.Tests.Tests.Components;

public class LiftableWorldObjectComponentTests
{
    private readonly Entity _entity1;
    private readonly Entity _entity2;
    private readonly LiftableWorldObjectComponent _liftableWorldObjectComponent;

    public LiftableWorldObjectComponentTests()
    {
        _entity1 = new("test1");
        _entity2 = new("test2");
        _entity1.AddComponent<Transform>();
        _entity2.AddComponent<Transform>();
        _entity1.AddComponent<TestElementComponent>();
        _liftableWorldObjectComponent = _entity1.AddComponent<LiftableWorldObjectComponent>();
    }

    [Fact]
    public void YouShouldBeAbleToLiftEntityAndDropEntity()
    {
        #region Act
        using var monitored = _liftableWorldObjectComponent.Monitor();
        var result1 = _liftableWorldObjectComponent.TryLift(_entity2);
        var wasOwner = _liftableWorldObjectComponent.Owner;
        var result2 = _liftableWorldObjectComponent.TryDrop();
        #endregion

        #region Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        wasOwner.Should().Be(_entity2);
        monitored.Should().Raise(nameof(LiftableWorldObjectComponent.Lifted));
        monitored.Should().Raise(nameof(LiftableWorldObjectComponent.Dropped));
        #endregion
    }

    [Fact]
    public void EntityCanBeLiftedOnce()
    {
        #region Act
        _liftableWorldObjectComponent.TryLift(_entity2);
        var result = _liftableWorldObjectComponent.TryLift(_entity2);
        #endregion

        #region Assert
        result.Should().BeFalse();
        #endregion
    }

    [Fact]
    public void EntityCanBeDroppedOnce()
    {
        #region Act
        _liftableWorldObjectComponent.TryLift(_entity2);
        _liftableWorldObjectComponent.TryDrop();
        var result = _liftableWorldObjectComponent.TryDrop();
        #endregion

        #region Assert
        result.Should().BeFalse();
        #endregion
    }

    [Fact]
    public void YouCanNotLiftYourself()
    {
        #region Act
        var result = _liftableWorldObjectComponent.TryLift(_liftableWorldObjectComponent.Entity);
        #endregion

        #region Assert
        result.Should().BeFalse();
        #endregion
    }

    [Fact]
    public void EntityShouldBeDroppedUponDispose()
    {
        #region Arrange
        using var monitored = _liftableWorldObjectComponent.Monitor();
        _liftableWorldObjectComponent.TryLift(_entity2);
        #endregion

        #region Act
        _entity2.Dispose();
        #endregion

        #region Assert
        _liftableWorldObjectComponent.Owner.Should().BeNull();
        monitored.Should().Raise(nameof(LiftableWorldObjectComponent.Lifted));
        monitored.Should().Raise(nameof(LiftableWorldObjectComponent.Dropped));
        #endregion
    }

    [Fact]
    public void EntityShouldBeDroppedUponDispose2()
    {
        #region Arrange
        using var monitored = _liftableWorldObjectComponent.Monitor();
        _liftableWorldObjectComponent.TryLift(_entity2);
        #endregion

        #region Act
        _entity2.Dispose();
        #endregion

        #region Assert
        _liftableWorldObjectComponent.Owner.Should().BeNull();
        monitored.Should().Raise(nameof(LiftableWorldObjectComponent.Lifted));
        monitored.Should().Raise(nameof(LiftableWorldObjectComponent.Dropped));
        #endregion
    }
}
