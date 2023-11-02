using RealmCore.Tests.Classes.Components;

namespace RealmCore.Tests.Tests.Components;

public class JobSessionComponentTests
{
    //[Fact]
    public void JobShouldCreateExpectedAmountOfPrivateAndPublicElements()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        var testJobComponent = player.AddComponentWithDI< TestJobComponent>();
        player.AddComponent(new JobStatisticsComponent(DateTime.Now));
        testJobComponent.CreateObjectives();
        var elements = realmTestingServer.GetRequiredService<IElementCollection>().GetAll().ToList();
        // TODO
    }
}
