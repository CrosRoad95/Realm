using SlipeServer.Server.Elements.ColShapes;
using RealmCore.Resources.ClientInterface;
using RealmCore.Server.Components.Elements.CollisionShapes;

namespace RealmCore.Tests.Tests.Components;

public class CollisionShapeElementComponentTests
{
    private readonly Entity _entity;
    private readonly CollisionSphereElementComponent _collisionSphereElementComponent;
    private readonly IClientInterfaceService _clientInterfaceServiceMock = new ClientInterfaceService();
    private readonly Mock<IElementCollection> _elementCollectionMock = new(MockBehavior.Strict);

    public CollisionShapeElementComponentTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IRealmConfigurationProvider>(new TestConfigurationProvider());
        services.AddSingleton<IECS, ECS>();
        services.AddSingleton(_elementCollectionMock.Object);
        services.AddSingleton(_clientInterfaceServiceMock);
        services.AddLogging(x => x.AddSerilog(new LoggerConfiguration().CreateLogger(), dispose: true));

        var serviceProvider = services.BuildServiceProvider();
        _entity = serviceProvider.GetRequiredService<IECS>().CreateEntity("test");
        _collisionSphereElementComponent = new(new CollisionSphere(new System.Numerics.Vector3(0, 0, 0), 10));
        _entity.AddComponent(_collisionSphereElementComponent);
    }

    [Fact]
    public void TestBasicCollisionDetection()
    {
        #region Arrange

        bool entityEntered = false;
        _collisionSphereElementComponent.EntityEntered += (enteredColshape, e) =>
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
