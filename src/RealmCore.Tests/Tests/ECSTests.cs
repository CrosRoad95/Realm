namespace RealmCore.Tests.Tests;

public class ECSTests
{
    private readonly IEntityEngine _ecs; 
    private readonly Mock<IElementCollection> _elementCollection = new();
    public ECSTests()
    {
        _ecs = new EntityEngine(_elementCollection.Object, null);
    }

    [Fact]
    public void AddAndRemoveEntityShouldWork()
    {
        #region Arrange & Act
        bool created = false;
        _ecs.EntityCreated += e =>
        {
            created = true;
        };
        var entity = _ecs.CreateEntity();
        entity.Dispose();
        #endregion

        #region Assert
        _ecs.Entities.Should().HaveCount(0);
        created.Should().BeTrue();
        #endregion
    }

    [Fact]
    public async Task TestThreadSafety()
    {
        #region Arrange & Act
        int createdEntities = 0;
        _ecs.EntityCreated += e =>
        {
            Interlocked.Increment(ref createdEntities);
        };

        await ParallelHelpers.Run((x,i) =>
        {
            var entity = _ecs.CreateEntity();
            entity.Dispose();
        });
        #endregion

        #region Assert
        createdEntities.Should().Be(8 * 100);
        _ecs.Entities.Should().HaveCount(0);
        #endregion
    }
}
