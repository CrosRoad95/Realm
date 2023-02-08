using Realm.Configuration;
using Realm.Domain;
using Realm.Domain.Components.CollisionShapes;
using SlipeServer.Server.Elements.ColShapes;
using FluentAssertions;
using Realm.Domain.Interfaces;
using Serilog;
using Realm.Resources.ClientInterface;

namespace Realm.Tests.Tests;

public class CollisionShapeElementComponentTests
{
    private readonly Entity _entity;
    private readonly CollisionSphereElementComponent _collisionSphereElementComponent;

    public CollisionShapeElementComponentTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton<RealmConfigurationProvider>(new TestConfigurationProvider());
        services.AddSingleton<ECS>();
        services.AddSingleton<ClientInterfaceService>();
        services.AddSingleton<IEntityByElement>(x => x.GetRequiredService<ECS>());
        services.AddLogging(x => x.AddSerilog(new LoggerConfiguration().CreateLogger(), dispose: true));

        var serviceProvider = services.BuildServiceProvider();
        _entity = serviceProvider.GetRequiredService<ECS>().CreateEntity("test", Entity.EntityTag.Unknown);
        _collisionSphereElementComponent = new(new CollisionSphere(new System.Numerics.Vector3(0,0,0), 10));
        _entity.AddComponent(_collisionSphereElementComponent);
    }

    [Fact]
    public void TestBasicCollisionDetection()
    {
        #region Arrange

        bool entityEntered = false;
        _collisionSphereElementComponent.EntityEntered += e =>
        {
            entityEntered = true;
        };
        #endregion

        #region Act
        _collisionSphereElementComponent.CheckCollisionWith(_entity);
        #endregion

        #region Assert
        entityEntered.Should().BeTrue();
        #endregion
    }
}
