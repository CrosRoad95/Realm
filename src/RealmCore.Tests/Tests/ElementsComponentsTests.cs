using RealmCore.Tests.Classes.Components;

namespace RealmCore.Tests.Tests;

public class ElementsComponentsTests
{
    [Fact]
    public void AddAndRemoveComponentShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        #region Arrange & Act
        bool componentAdded = false;
        bool componentDetached = false;
        player.Components.ComponentAdded += e =>
        {
            componentAdded = true;
        };
        player.Components.ComponentDetached += e =>
        {
            componentDetached = true;
        };
        var component = new TestComponent();
        player.AddComponent(component);
        player.DestroyComponent(component);
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
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        #region Arrange & Act
        int addedComponents = 0;
        player.Components.ComponentAdded += e =>
        {
            Interlocked.Increment(ref addedComponents);
        };

        await ParallelHelpers.Run(() =>
        {
            var component = new TestComponent();
            player.Components.AddComponent(component);
            player.Components.DestroyComponent(component);
        });
        #endregion

        #region Assert
        addedComponents.Should().Be(8 * 100);
        player.Components.ComponentsList.Should().BeEmpty();
        #endregion
    }
    
    [Fact]
    public void ComponentShouldNotBeAddedIfFailedToLoad()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        #region Arrange & Act
        var component = new ThrowExceptionComponent();

        var action = () => player.AddComponent(component);
        #endregion

        #region Act
        action.Should().Throw<Exception>().WithMessage("Something went wrong");
        player.Components.ComponentsList.Should().BeEmpty();
        #endregion
    }

    [Fact]
    public void ComponentUsageShouldPreventYouFromAddingOneComponentTwoTimes()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        #region Arrange
        var action = () => player.AddComponent<OneComponent>();
        #endregion

        #region Act & Assert
        action.Should().NotThrow();
        action.Should().Throw<ComponentCanNotBeAddedException<OneComponent>>().WithMessage("Only one instance of component 'OneComponent' can be added to one element");
        #endregion
    }

    [Fact]
    public void ComponentShouldBeAbleToDisposeOtherComponentsInDisposeMethod()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        player.AddComponent<ParentComponent>();
        player.AddComponent<ChildComponent>();
        #endregion

        #region Act
        player.TryDestroyComponent<ParentComponent>().Should().BeTrue();
        #endregion

        #region Assert
        player.Components.ComponentsList.Should().BeEmpty();
        #endregion
    }

    [Fact]
    public void ComponentShouldBeAbleToDisposeOtherComponentsInDispose2Method()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        var test1 = player.AddComponent<TestComponent>();
        var test2 = player.AddComponent<TestComponent>();

        test1.Detached += e =>
        {
            player.Components.TryDestroyComponent(test2).Should().BeTrue();
        };
        #endregion

        #region Act
        player.Components.TryDestroyComponent(test1).Should().BeTrue();
        #endregion

        #region Assert
        player.Components.ComponentsList.Should().BeEmpty();
        #endregion
    }

    [Fact]
    public void ComponentShouldBeAbleToDisposeOtherComponentsInDispose3Method()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        var test1 = player.AddComponent<TestComponent>();
        var test2 = player.AddComponent<ParentComponent>();

        test1.Detached += e =>
        {
            player.Components.TryDestroyComponent<ParentComponent>().Should().BeTrue();
        };
        #endregion

        #region Act
        player.Components.TryDestroyComponent(test1).Should().BeTrue();
        #endregion

        #region Assert
        player.Components.ComponentsList.Should().BeEmpty();
        #endregion
    }

    [Fact]
    public void TestElementAndItsComponentLifecycle1()
    {
        #region Arrange & Act
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        var component = new TestComponent();
        using var componentsMonitor = player.Components.Monitor();
        using var componentMonitor = component.Monitor();
        player.AddComponent(component);
        player.DestroyComponent(component);
        #endregion

        #region Act
        var elementEvents = componentsMonitor.GetEvents("Element");
        var componentEvents = componentMonitor.GetEvents("Component");

        var occurredEvents = elementEvents
            .Concat(componentEvents)
            .OrderBy(x => x.TimestampUtc)
            .Select(x => x.Name)
            .ToList();

        var expectedEvents = new List<string> { "Component/Attached", "Element/ComponentAdded", "Component/Detached", "Element/ComponentDetached" };

        occurredEvents.Should().Equal(expectedEvents);
        #endregion
    }

    [Fact]
    public void TestElementAndItsComponentLifecycle2()
    {
        #region Arrange & Act
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        var component = new TestComponent();
        using var elementMonitor = player.Components.Monitor();
        using var componentMonitor = component.Monitor();
        player.AddComponent(component);
        player.Destroy();
        #endregion

        #region Act
        var elementEvents = elementMonitor.GetEvents("Element");
        var componentEvents = componentMonitor.GetEvents("Component");

        var occurredEvents = elementEvents
            .Concat(componentEvents)
            .OrderBy(x => x.TimestampUtc)
            .Select(x => x.Name)
            .ToList();

        var expectedEvents = new List<string> { "Component/Attached", "Element/ComponentAdded", "Component/Detached", "Element/ComponentDetached" };

        occurredEvents.Should().Equal(expectedEvents);
        #endregion
    }
}
