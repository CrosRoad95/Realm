namespace RealmCore.Tests.Integration.Players;

public class GroupServiceTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>, IDisposable
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingPlayer _player;
    private readonly PlayerGroupsFeature _groups;
    private readonly GroupsService _groupsService;

    public GroupServiceTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _groups = _player.Groups;
        _groupsService = _fixture.Hosting.GetRequiredService<GroupsService>();
    }

    [Fact]
    private async Task GroupCanBeCreated()
    {
        var name = Guid.NewGuid().ToString();

        var result = await _groupsService.Create(name);

        ((GroupsResults.Created)result.Value).group.Name.Should().Be(name);
    }

    public void Dispose()
    {

    }
}
