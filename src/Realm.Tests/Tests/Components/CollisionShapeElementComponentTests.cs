using SlipeServer.Server.Elements.ColShapes;
using Realm.Domain.Interfaces;
using Realm.Resources.ClientInterface;
using Realm.Domain.Components.Elements.CollisionShapes;

namespace Realm.Tests.Tests.Components;

public class CollisionShapeElementComponentTests
{
    private readonly Entity _entity;
    private readonly CollisionSphereElementComponent _collisionSphereElementComponent;
    private readonly IClientInterfaceService _clientInterfaceServiceMock = new ClientInterfaceService();

    public CollisionShapeElementComponentTests()
    {
        // FocusableAdded
        var services = new ServiceCollection();
        services.AddSingleton<IRealmConfigurationProvider>(new TestConfigurationProvider());
        services.AddSingleton<ECS>();
        services.AddSingleton(_clientInterfaceServiceMock);
        services.AddSingleton<IEntityByElement>(x => x.GetRequiredService<ECS>());
        services.AddLogging(x => x.AddSerilog(new LoggerConfiguration().CreateLogger(), dispose: true));

        var serviceProvider = services.BuildServiceProvider();
        _entity = serviceProvider.GetRequiredService<ECS>().CreateEntity("test", Entity.EntityTag.Unknown);
        _collisionSphereElementComponent = new(new CollisionSphere(new System.Numerics.Vector3(0, 0, 0), 10));
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
