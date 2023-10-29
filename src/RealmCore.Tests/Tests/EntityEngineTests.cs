namespace RealmCore.Tests.Tests;

public class EntityEngineTests
{
    private readonly IEntityEngine _entityEngine; 
    private readonly Mock<IElementCollection> _elementCollection = new();
    public EntityEngineTests()
    {
        _entityEngine = new EntityEngine(_elementCollection.Object, null);
    }

    [Fact]
    public void AddAndRemoveEntityShouldWork()
    {
        #region Arrange & Act
        bool created = false;
        _entityEngine.EntityCreated += e =>
        {
            created = true;
        };
        var entity = _entityEngine.CreateEntity();
        entity.Dispose();
        #endregion

        #region Assert
        _entityEngine.Entities.Should().HaveCount(0);
        created.Should().BeTrue();
        #endregion
    }

    [Fact]
    public async Task TestThreadSafety()
    {
        #region Arrange & Act
        int createdEntities = 0;
        _entityEngine.EntityCreated += e =>
        {
            Interlocked.Increment(ref createdEntities);
        };

        await ParallelHelpers.Run((x,i) =>
        {
            var entity = _entityEngine.CreateEntity();
            entity.Dispose();
        });
        #endregion

        #region Assert
        createdEntities.Should().Be(8 * 100);
        _entityEngine.Entities.Should().HaveCount(0);
        #endregion
    }
}
