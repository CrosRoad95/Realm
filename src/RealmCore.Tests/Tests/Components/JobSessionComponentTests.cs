using RealmCore.Resources.GuiSystem;
using RealmCore.Server.Components.Elements;
using RealmCore.Server.Components.Elements.CollisionShapes;
using RealmCore.Server.Factories;
using RealmCore.Tests.Classes.Components;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;

namespace RealmCore.Tests.Tests.Components;

public class JobSessionComponentTests
{
    private readonly IElementCollection _elementCollection;
    private readonly EntityHelper _entityHelper;
    private readonly Mock<IRPGServer> _rpgServerMock = new(MockBehavior.Strict);
    private readonly RealmTestingServer _realmTestingServer;
    private readonly Mock<IGuiSystemService> _guiSystemService = new(MockBehavior.Strict);

    public JobSessionComponentTests()
    {
        _rpgServerMock.Setup(x => x.IsReady).Returns(true);
        _rpgServerMock.Setup(x => x.AssociateElement(It.IsAny<object>())).Callback<object>(e =>
        {
            _elementCollection.Add((Element)e);
        });
        _realmTestingServer = new(null, services =>
        {
            services.AddSingleton<IEntityFactory, EntityFactory>();
            services.AddSingleton(_guiSystemService.Object);
            services.AddSingleton(_rpgServerMock.Object);
        });
        _entityHelper = new(_realmTestingServer);
        _elementCollection = _realmTestingServer.GetRequiredService<IElementCollection>();
    }

    [Fact]
    public void JobShouldCreateExpectedAmountOfPrivateAndPublicElements()
    {
        var playerEntity = _entityHelper.CreatePlayerEntity();
        var testJobComponent = playerEntity.AddComponent<TestJobComponent>();
        playerEntity.AddComponent(new JobStatisticsComponent(DateTime.Now));
        testJobComponent.CreateObjectives();
        var elements = _elementCollection.GetAll().ToList();
        var components =  playerEntity.Components.ToList();
        components.OfType<PlayerPrivateElementComponent<MarkerElementComponent>>().Should().HaveCount(4);
        components.OfType<PlayerPrivateElementComponent<CollisionSphereElementComponent>>().Should().HaveCount(4);
        components.OfType<PlayerPrivateElementComponent<BlipElementComponent>>().Should().HaveCount(1);
    }
}
