namespace RealmCore.Tests.Tests;

public class ECSTests
{
    private readonly ECS _ecs; 
    private readonly Mock<IElementCollection> _elementCollection = new();
    private readonly Mock<ILogger<Entity>> _logger = new(MockBehavior.Strict);
    public ECSTests()
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
        // .BeginScope<Dictionary<string, object>>(Dictionary<string, object>)
        _ecs = new ECS(services.BuildServiceProvider(), _elementCollection.Object, null);
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
        var entity = _ecs.CreateEntity("foo", EntityTag.Unknown);
        entity.Dispose();
        #endregion

        #region Assert
        _ecs.Entities.Should().HaveCount(1);
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
            var entity = _ecs.CreateEntity($"foo{x}{i}", EntityTag.Unknown);
            entity.Dispose();
        });
        #endregion

        #region Assert
        createdEntities.Should().Be(8 * 100);
        _ecs.Entities.Should().HaveCount(1);
        #endregion
    }
}
