﻿using RealmCore.Tests.Classes.Components;

namespace RealmCore.Tests.Tests;

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
        var entity = new Entity(_serviceProvider, "foo", EntityTag.Unknown);
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
        var entity = new Entity(_serviceProvider, "foo", EntityTag.Unknown);
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
        var entity = new Entity(_serviceProvider, "foo", EntityTag.Unknown);
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
        var entity = new Entity(_serviceProvider, "foo", EntityTag.Unknown);
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
        var entity = new Entity(_serviceProvider, "foo", EntityTag.Unknown);
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
        var entity = new Entity(_serviceProvider, "foo", EntityTag.Unknown);
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
        var entity = new Entity(_serviceProvider, "foo", EntityTag.Unknown);
        var component = new ThrowExceptionAsyncComponent();

        #endregion

        #region Act
        var action = () => entity.AddComponent(component);
        #endregion

        #region Act
        action.Should().Throw<ArgumentException>().WithMessage("Can not add async component to non async entity");
        #endregion
    }

    [Fact]
    public void ComponentUsageShouldPreventYouFromAddingOneComponentTwoTimes()
    {
        #region Arrange
        var entity = new Entity(_serviceProvider, "foo", EntityTag.Unknown);
        var action = () => entity.AddComponent<OneComponent>();
        #endregion

        #region Act & Assert
        action.Should().NotThrow();
        action.Should().Throw<ComponentCanNotBeAddedException<OneComponent>>().WithMessage("Only one instance of component 'OneComponent' can be added to one entity");
        #endregion
    }
}