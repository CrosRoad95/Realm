﻿using FluentAssertions;
using Realm.Tests.Helpers;

namespace Realm.Tests.Tests;

public class ECSTests
{
    private readonly ECS _ecs; 
    public ECSTests()
    {
        var services = new ServiceCollection();
        _ecs = new ECS(services.BuildServiceProvider());
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
        var entity = _ecs.CreateEntity("foo", "test");
        entity.Dispose();
        #endregion

        #region Assert
        _ecs.Entities.Should().BeEmpty();
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
            var entity = _ecs.CreateEntity($"foo{x}{i}", "test");
            entity.Dispose();
        });
        #endregion

        #region Assert
        createdEntities.Should().Be(8 * 100);
        _ecs.Entities.Should().BeEmpty();
        #endregion
    }
}