using FluentAssertions;
using Realm.Domain;
using Realm.Tests.Classes.Components;
using Realm.Tests.Helpers;

namespace Realm.Tests.Tests;

public class EntityTests
{
    private readonly IServiceProvider _serviceProvider;
    public EntityTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new object());
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void AddAndRemoveComponentShouldWork()
    {
        #region Arrange & Act
        bool componentAdded = false;
        bool componentDetached = false;
        var entity = new Entity(_serviceProvider, "foo", "test");
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
        component.IsDispoed().Should().BeTrue();
        #endregion
    }

    [Fact]
    public async Task TestThreadSafety()
    {
        #region Arrange & Act
        int addedComponents = 0;
        var entity = new Entity(_serviceProvider, "foo", "test");
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
        var entity = new Entity(_serviceProvider, "foo", "test");
        var component = new TestComponent();
        entity.AddComponent(component);
        #endregion

        #region Act
        component.isObjectDefined().Should().BeTrue();
        #endregion
    }

}
