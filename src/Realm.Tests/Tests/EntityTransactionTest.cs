using Realm.Tests.Classes.Components;

namespace Realm.Tests.Tests;

public class EntityTransactionTest
{
    // TestComponent
    private readonly Entity _entity;
    public EntityTransactionTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new object());
        var serviceProvider = services.BuildServiceProvider();

        _entity = new(serviceProvider, "test", Entity.EntityTag.Unknown);
    }

    [Fact]
    public void TransactionShouldBeCommitable()
    {
        #region Act
        using var transaction = _entity.BeginComponentTransaction();
        _entity.AddComponent<TestComponent>();
        _entity.AddComponent<TestComponent>();
        var addedComponents1 = _entity.Commit(transaction);

        using var transaction2 = _entity.BeginComponentTransaction();
        _entity.AddComponent<TestComponent>();
        _entity.AddComponent<TestComponent>();
        var addedComponents2 = _entity.Commit(transaction2);
        #endregion

        #region Assert
        addedComponents1.Should().Be(2);
        addedComponents2.Should().Be(2);
        _entity.Components.Should().HaveCount(4);
        #endregion
    }
    
    [Fact]
    public void TransactionShouldBeRollbackable()
    {
        #region Act
        using var transaction = _entity.BeginComponentTransaction();
        _entity.AddComponent<TestComponent>();
        _entity.AddComponent<TestComponent>();
        var rollbackedComponents1 = _entity.Rollback(transaction);

        using var transaction2 = _entity.BeginComponentTransaction();
        _entity.AddComponent<TestComponent>();
        _entity.AddComponent<TestComponent>();
        var rollbackedComponents2 = _entity.Rollback(transaction2);
        #endregion

        #region Assert
        rollbackedComponents1.Should().Be(2);
        rollbackedComponents2.Should().Be(2);
        _entity.Components.Should().BeEmpty();
        #endregion
    }

    [Fact]
    public void YouCanNotStartTwoTransactionsForSingleEntity()
    {
        #region Arrange
        var act = () =>
        {
            using var a = _entity.BeginComponentTransaction();
            using var b = _entity.BeginComponentTransaction();
        };
        #endregion

        #region Act & Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Transaction wasn't commited properly");
        #endregion
    }

    [Fact]
    public void TransactionCanBeCommitedOnlyOnce()
    {
        #region Arrange
        var act = () =>
        {
            using var transaction = _entity.BeginComponentTransaction();
            _entity.Commit(transaction);
            _entity.Commit(transaction);
        };
        #endregion

        #region Act & Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Transaction already commited");
        #endregion
    }

    [Fact]
    public void TransactionCanBeRollbackedOnlyOnce()
    {
        #region Arrange
        var act = () =>
        {
            using var transaction = _entity.BeginComponentTransaction();
            _entity.Rollback(transaction);
            _entity.Rollback(transaction);
        };
        #endregion

        #region Act & Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Transaction already commited");
        #endregion
    }
}
