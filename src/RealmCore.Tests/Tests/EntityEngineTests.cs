namespace RealmCore.Tests.Tests;

public class EntityEngineTests
{
    private readonly IElementFactory _elementFactory; 
    private readonly Mock<IElementCollection> _elementCollection = new();
    public EntityEngineTests()
    {
        _elementFactory = new EntityEngine(_elementCollection.Object, null);
    }

    [Fact]
    public void AddAndRemoveEntityShouldWork()
    {
        #region Arrange & Act
        bool created = false;
        _elementFactory.EntityCreated += e =>
        {
            created = true;
        };
        var entity = _elementFactory.CreateEntity();
        entity.Dispose();
        #endregion

        #region Assert
        _elementFactory.Entities.Should().HaveCount(0);
        created.Should().BeTrue();
        #endregion
    }

    [Fact]
    public async Task TestThreadSafety()
    {
        #region Arrange & Act
        int createdEntities = 0;
        _elementFactory.EntityCreated += e =>
        {
            Interlocked.Increment(ref createdEntities);
        };

        await ParallelHelpers.Run((x,i) =>
        {
            var entity = _elementFactory.CreateEntity();
            entity.Dispose();
        });
        #endregion

        #region Assert
        createdEntities.Should().Be(8 * 100);
        _elementFactory.Entities.Should().HaveCount(0);
        #endregion
    }
}
