using RealmCore.Tests.Classes.Components;

namespace RealmCore.Tests.Tests;

public class EntityTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RealmTestingServer _server;
    private readonly EntityHelper _entityHelper;

    private readonly Mock<ILogger<Entity>> _logger = new(MockBehavior.Strict);
    public EntityTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new object());
        _logger.SetupLogger();
        services.AddSingleton(_logger.Object);
        _serviceProvider = services.BuildServiceProvider();
        _server = new();
        _entityHelper = new(_server);
    }

    [Fact]
    public void AddAndRemoveComponentShouldWork()
    {
        #region Arrange & Act
        bool componentAdded = false;
        bool componentDetached = false;
        var entity = new Entity(_serviceProvider, "foo");
        entity.ComponentAdded += e =>
        {
            componentAdded = true;
        };
        entity.ComponentDetached += e =>
        {
            componentDetached = true;
        };
        var component = new TestComponent();
        entity.AddComponent(component);
        entity.DestroyComponent(component);
        #endregion

        #region Act
        componentAdded.Should().BeTrue();
        componentDetached.Should().BeTrue();
        component.IsDisposed().Should().BeTrue();
        #endregion
    }

    [Fact]
    public async Task TestThreadSafety()
    {
        #region Arrange & Act
        int addedComponents = 0;
        var entity = new Entity(_serviceProvider, "foo");
        entity.ComponentAdded += e =>
        {
            Interlocked.Increment(ref addedComponents);
        };

        await ParallelHelpers.Run(() =>
        {
            var component = new TestComponent();
            entity.AddComponent(component);
            entity.DestroyComponent(component);
        });
        #endregion

        #region Assert
        addedComponents.Should().Be(8 * 100);
        entity.Components.Should().BeEmpty();
        #endregion
    }

    [Fact]
    public void TestComponentsDependencyInjection()
    {
        #region Arrange & Act
        var entity = new Entity(_serviceProvider, "foo");
        var component = new TestComponent();
        entity.AddComponent(component);
        #endregion

        #region Act
        component.isObjectDefined().Should().BeTrue();
        #endregion
    }
    
    [Fact]
    public void ComponentShouldNotBeAddedIfFailedToLoad()
    {
        #region Arrange & Act
        var entity = new Entity(_serviceProvider, "foo");
        var component = new ThrowExceptionComponent();

        var action = () => entity.AddComponent(component);
        #endregion

        #region Act
        action.Should().Throw<Exception>().WithMessage("Something went wrong");
        entity.Components.Should().BeEmpty();
        #endregion
    }

    [Fact]
    public async Task ComponentShouldNotBeAddedIfFailedToAsyncLoad()
    {
        #region Arrange & Act
        var entity = new Entity(_serviceProvider, "foo");
        var component = new ThrowExceptionAsyncComponent();

        var action = async () => await entity.AddComponentAsync(component);
        #endregion

        #region Act
        await action.Should().ThrowAsync<Exception>().WithMessage("Something went wrong");
        entity.Components.Should().BeEmpty();
        #endregion
    }

    [Fact]
    public async Task ComponentShouldBeRemovedIfAsyncLoadThrowException()
    {
        #region Arrange & Act
        var entity = new Entity(_serviceProvider, "foo");
        var component = new ThrowExceptionAsyncComponent();

        #endregion

        #region Act
        var action = async () => await entity.AddComponentAsync(component);
        #endregion

        #region Assert
        var t = new TaskCompletionSource();
        entity.ComponentDetached += e =>
        {
            t.SetResult();
        };

        _ = action();

        (await Task.WhenAny(t.Task, Task.Delay(5000))).Should().Be(t.Task);
        #endregion
    }

    [Fact]
    public void AsyncComponentCanNotBeAddedToNonAsyncEntity()
    {
        #region Arrange
        var entity = new Entity(_serviceProvider, "foo");
        var component = new ThrowExceptionAsyncComponent();

        #endregion

        #region Act
        var action = () => entity.AddComponent(component);
        #endregion

        #region Act
        action.Should().Throw<ArgumentException>().WithMessage("Can not add async component using sync method");
        #endregion
    }

    [Fact]
    public void ComponentUsageShouldPreventYouFromAddingOneComponentTwoTimes()
    {
        #region Arrange
        var entity = new Entity(_serviceProvider, "foo");
        var action = () => entity.AddComponent<OneComponent>();
        #endregion

        #region Act & Assert
        action.Should().NotThrow();
        action.Should().Throw<ComponentCanNotBeAddedException<OneComponent>>().WithMessage("Only one instance of component 'OneComponent' can be added to one entity");
        #endregion
    }

    [Fact]
    public void ComponentShouldBeAbleToDisposeOtherComponentsInDisposeMethod()
    {
        #region Arrange
        var entity = new Entity(_serviceProvider, "foo");
        entity.AddComponent<ParentComponent>();
        entity.AddComponent<ChildComponent>();
        #endregion

        #region Act
        entity.TryDestroyComponent<ParentComponent>().Should().BeTrue();
        #endregion

        #region Assert
        entity.Components.Should().BeEmpty();
        #endregion
    }

    [Fact]
    public void ComponentShouldBeAbleToDisposeOtherComponentsInDispose2Method()
    {
        #region Arrange
        var entity = new Entity(_serviceProvider, "foo");
        var test1 = entity.AddComponent<TestComponent>();
        var test2 = entity.AddComponent<TestComponent>();

        test1.Disposed += e =>
        {
            entity.TryDestroyComponent(test2).Should().BeTrue();
        };
        #endregion

        #region Act
        entity.TryDestroyComponent(test1).Should().BeTrue();
        #endregion

        #region Assert
        entity.Components.Should().BeEmpty();
        #endregion
    }

    [Fact]
    public void ComponentShouldBeAbleToDisposeOtherComponentsInDispose3Method()
    {
        #region Arrange
        var entity = new Entity(_serviceProvider, "foo");
        var test1 = entity.AddComponent<TestComponent>();
        var test2 = entity.AddComponent<ParentComponent>();

        test1.Disposed += e =>
        {
            entity.TryDestroyComponent<ParentComponent>().Should().BeTrue();
        };
        #endregion

        #region Act
        entity.TryDestroyComponent(test1).Should().BeTrue();
        #endregion

        #region Assert
        entity.Components.Should().BeEmpty();
        #endregion
    }

    [Fact]
    public void TryGetElementTest()
    {
        #region Arrange
        var elementEntity = _entityHelper.CreateObjectEntity();
        #endregion

        #region Act
        var success = elementEntity.TryGetElement(out var element);
        #endregion

        #region Assert
        success.Should().BeTrue();
        #endregion
    }

    [Fact]
    public void TestEntityAndItsComponentLifecycle1()
    {
        #region Arrange & Act
        var entity = new Entity(_serviceProvider, "foo");
        var component = new TestComponent();
        using var entityMonitor = entity.Monitor();
        using var componentMonitor = component.Monitor();
        entity.AddComponent(component);
        entity.DestroyComponent(component);
        #endregion

        #region Act
        var entityEvents = entityMonitor.GetEvents("Entity");
        var componentEvents = componentMonitor.GetEvents("Component");

        var occurredEvents = entityEvents
            .Concat(componentEvents)
            .OrderBy(x => x.TimestampUtc)
            .Select(x => x.Name)
            .ToList();

        var expectedEvents = new List<string> { "Entity/ComponentAdded", "Entity/ComponentDetached", "Component/DetachedFromEntity", "Component/Disposed" };

        occurredEvents.Should().Equal(expectedEvents);
        #endregion
    }

    [Fact]
    public void TestEntityAndItsComponentLifecycle2()
    {
        #region Arrange & Act
        var entity = new Entity(_serviceProvider, "foo");
        var component = new TestComponent();
        using var entityMonitor = entity.Monitor();
        using var componentMonitor = component.Monitor();
        entity.AddComponent(component);
        entity.Dispose();
        #endregion

        #region Act
        var entityEvents = entityMonitor.GetEvents("Entity");
        var componentEvents = componentMonitor.GetEvents("Component");

        var occurredEvents = entityEvents
            .Concat(componentEvents)
            .OrderBy(x => x.TimestampUtc)
            .Select(x => x.Name)
            .ToList();

        var expectedEvents = new List<string> { "Entity/ComponentAdded", "Entity/PreDisposed", "Entity/ComponentDetached", "Component/DetachedFromEntity", "Component/Disposed", "Entity/Disposed" };

        occurredEvents.Should().Equal(expectedEvents);
        #endregion
    }
}
